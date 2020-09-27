using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using NodeMachine.Nodes;
using NodeMachine.States;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeMachine {

    [ExecuteAlways]
    [Serializable]
    public class Machine : MonoBehaviour
    {

        public NodeMachineModel _model;

        [NonSerialized]
        [HideInInspector]
        public UnityEngine.Object propsObject;
        public bool optimiseParallel = true;
        private HashSet<RunnableNode> _currentRunnables;
        public HashSet<RunnableNode> CurrentRunnables
        {
            get { return _currentRunnables; }
        }
        private HashSet<Link> _currentLinks = new HashSet<Link>();
        public HashSet<Link> CurrentLinks
        {
            get { return _currentLinks; }
        }
        private Dictionary<Node, NodeFollower> followers;
        private float _lastCheckinTime = 0f;
        public bool triggerModelCheckinEvent = false;
        private bool loadedInitialProps;
        public delegate void MachineChangeEvent ();
        public event MachineChangeEvent OnMachineChange;
        public event Action OnCheckin;
        public bool recordNodePaths;

        void OnEnable () {
            if (_model != null)
                ReloadProperties();
        }

        public void ReloadProperties () {
            _model.ForceReloadProperties();
            Component[] objs = GetComponents<Component>();
            foreach (UnityEngine.Object obj in objs) {
                if (obj == null)
                    continue;
                MachinePropsAttribute attr = obj.GetType().GetCustomAttribute<MachinePropsAttribute>();
                if (attr != null) {
                    if (attr.Model == _model.name) {
                        propsObject = obj;
                        break;
                    }
                }
            }
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                _currentRunnables = new HashSet<RunnableNode>();
                followers = new Dictionary<Node, NodeFollower>();
                _model.ReloadOnce();
                foreach (Node node in _model.GetNodes())
                {
                    node.OnGameStart(this);
                    if (node is ActiveNode) {
                        Debug.Log("Found active node! Adding follower");
                        followers.Add(node, new NodeFollower(this, node, true));
                    }
                }
                Node entry = _model.GetNodes<EntryNode>()[0];
                followers.Add(entry, new NodeFollower(this, entry));
            }
        }

        void OnValidate () {
#if UNITY_EDITOR
            if (!optimiseParallel && !EditorApplication.isPlayingOrWillChangePlaymode) {
                if (!EditorUtility.DisplayDialog("WARNING", "Using machines without the Optimise Parallel feature makes them LOOP UNSAFE. Are you sure you want to do this?", "Yes", "No")) {
                    optimiseParallel = true;
                }
            }
#endif
            if (_model != null) {
                if (propsObject != null && !_model.machinePropertiesDelegates.ContainsKey(propsObject)) {
                    ReloadProperties();
                }
            }
            if (OnMachineChange != null) {
                OnMachineChange.Invoke();
            }
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                if (Time.time == 0 || Time.time - _lastCheckinTime >= _model.CheckinTime)
                {
                    Checkin();
                    _lastCheckinTime = Time.time;
                    string crStr = "Checkin at " + Time.time + " yielded runnables: ";
                    foreach (RunnableNode runnable in _currentRunnables) {
                        crStr += runnable.ToString() + " ";
                    }
                    Debug.Log(crStr);
                    string fStr = "Checkin at " + Time.time + " tested from nodes: ";
                    foreach (Node node in followers.Keys) {
                        fStr += node.ToString() + " ";
                    }
                    Debug.Log(fStr);
                }
            }
        }

        void Checkin () {
            _currentRunnables.Clear();
            _currentLinks.Clear();
            foreach (NodeFollower follower in followers.Values) {
                NodeFollower.CurrentFollowState? followStateNullable = follower.Checkin();
                if (followStateNullable != null) {
                    NodeFollower.CurrentFollowState followState = (NodeFollower.CurrentFollowState) followStateNullable;
                    AddCurrentRunnables(followState.runnables);
                    AddCurrentLinks(followState.links);
                }
            }
            if (triggerModelCheckinEvent)
                _model.TriggerCheckinEvent();
            if (OnCheckin != null)
                OnCheckin();
        }

        public void MakeEditorCheckinEventTarget(bool isTarget)
        {
            triggerModelCheckinEvent = isTarget;
        }

        void SetCurrentRunnables(HashSet<RunnableNode> runnables)
        {
            _currentRunnables = runnables;
        }

        public void AddCurrentRunnables(HashSet<RunnableNode> runnables)
        {
            foreach (RunnableNode node in runnables) {
                _currentRunnables.Add(node);
            }
        }

        public void AddCurrentLinks(HashSet<Link> links)
        {
            foreach (Link link in links) {
                _currentLinks.Add(link);
            }
        }

        public void FinishFollower (Node start) {
            followers.Remove(start);
        }

        public void SetModel(NodeMachineModel model)
        {
            this._model = model;
        }
        public NodeMachineModel GetModel()
        {
            return _model;
        }
        
        // These wrappers are left unsafe on purpose:
        // Nothing should be trying to access non-existent fields,
        // If they are then there should be complaints.
        public object GetProp(string fieldName) {
            return _model.machinePropertiesDelegates[propsObject][fieldName].getter();
        }

        public void SetProp(string fieldName, object value) {
            _model.machinePropertiesDelegates[propsObject][fieldName].setter(value);
        }

    }

}