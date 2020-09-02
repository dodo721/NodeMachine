using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NodeMachine.Nodes;
using NodeMachine.States;

namespace NodeMachine {

    [ExecuteAlways]
    [Serializable]
    public class Machine : MonoBehaviour
    {

        public NodeMachineModel _model;

        [HideInInspector]
        public NodeMachineProperties properties;

        public bool optimiseParallel = true;
        private ModelLoader _loader;
        private Dictionary<StateNode, State> stateInstances = new Dictionary<StateNode, State>();
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
        private HashSet<Node> triedNodes = new HashSet<Node>();
        public event Action<State> OnStateChange;
        private float _lastCheckinTime = 0f;
        private bool triggerModelCheckinEvent = false;
        private bool loadedInitialProps;
        public delegate void MachineChangeEvent ();
        public event MachineChangeEvent OnMachineChange;

        void Start()
        {
            LoadModel();
            if (Application.isPlaying)
            {
                foreach (Node node in _model.GetNodes())
                {
                    node.OnGameStart(this);
                }
                // TODO: sometimes not working??
                HashSet<RunnableNode> startRunnables = new HashSet<RunnableNode>();
                startRunnables.Add(_model.GetNodes<EntryStateNode>()[0] as RunnableNode);
                //Debug.Log("Model " + _model.name + " has " + startRunnables.Count + " entry nodes");
                SetCurrentRunnables(startRunnables);
            }
        }

        void OnEnable () {
            ReloadEvents();
        }

        void OnDisable () {
            RemoveEvents();
        }

        void OnValidate () {
            StartCoroutine (DelayedReload());
            if (OnMachineChange != null) {
                OnMachineChange.Invoke();
            }
        }

        IEnumerator DelayedReload () {
            yield return new WaitForEndOfFrame();
            ReloadAll();
        }

        // TODO: needed???
        void LoadModel()
        {
            if (_model != null)
            {
                
            }
        }

        public void ReloadProperties () {
            if (_model == null) {
                foreach (NodeMachineProperties props in GetComponents<NodeMachineProperties>()) {
                    DestroyImmediate(props);
                }
                return;
            }
            bool modelTypeIsPresent = false;
            foreach (NodeMachineProperties props in GetComponents<NodeMachineProperties>()) {
                if (props.name == _model._propertyType?.ToString()) {
                    modelTypeIsPresent = true;
                } else {
                    DestroyImmediate(props);
                }
            }
            if (!modelTypeIsPresent && _model._propertyType != null) {
                if (gameObject.GetComponent(_model._propertyType) == null)
                    gameObject.AddComponent(_model._propertyType);
            }
            if (_model._propertyType != null && gameObject != null)
                properties = gameObject.GetComponent(_model._propertyType) as NodeMachineProperties;
        }

        void ReloadEvents()
        {
            if (_model == null)
                return;
            _model.OnPropsReload -= ReloadProperties;
            _model.OnPropsReload += ReloadProperties;
            _model.OnPropValueChange -= ChangePropVal;
            _model.OnPropValueChange += ChangePropVal;
        }

        void RemoveEvents()
        {
            if (_model == null)
                return;
            _model.OnPropsReload -= ReloadProperties;
            _model.OnPropValueChange -= ChangePropVal;
        }

        public void ReloadModel()
        {
            LoadModel();
            ReloadProperties();
        }

        private void ReloadAll()
        {
            ReloadModel();
            ReloadEvents();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void ReloadAllMachines()
        {
            foreach (Machine machine in FindObjectsOfType<Machine>())
            {
                machine.ReloadAll();
            }
        }

        void ChangePropVal (string name, dynamic value) {
            properties.SetProp(name, value);
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                if (Time.time == 0 || Time.time - _lastCheckinTime >= _model.CheckinTime)
                {
                    Checkin();
                    _lastCheckinTime = Time.time;
                }
            }
        }

        void Checkin()
        {
            // Run checkin for current runnables
            foreach (RunnableNode runnable in _currentRunnables)
                runnable.Checkin(this);
            // Test the model for the next node.
            DoNodeFollow();
            if (triggerModelCheckinEvent)
                _model.TriggerCheckinEvent();
        }

        public void MakeEditorCheckinEventTarget(bool isTarget)
        {
            triggerModelCheckinEvent = isTarget;
        }

        /// <summary>
        ///  Begins a branched follow through the node network.
        ///  Returns true if new RunnableNodes were chosen.
        /// </summary>
        bool DoNodeFollow()
        {
            _currentLinks.Clear();
            triedNodes.Clear();
            HashSet<RunnableNode> nextNodes = new HashSet<RunnableNode>();
            foreach (RunnableNode node in _currentRunnables) {
                HashSet<RunnableNode> branchedNodes = FollowNode(node, null, node);
                foreach (RunnableNode branchedNode in branchedNodes) {
                    nextNodes.Add(branchedNode);
                }
            }
            HashSet<RunnableNode> nodesChanged = NodeHashSetsDiff(_currentRunnables, nextNodes);
            if (nodesChanged.Count != 0)
            {
                // If the HashSet of current nodes has changed,
                // Trigger RunEnd and Start events on the differences
                foreach (RunnableNode node in nodesChanged) {
                    if (!_currentRunnables.Contains(node)) {
                        node.OnRunStart(this);
                    } else if (!nextNodes.Contains(node)) {
                        node.OnRunEnd(this);
                    } else
                        Debug.LogWarning("Node " + node + " marked as changed but not changed in current nodes or new nodes!");
                }
                SetCurrentRunnables(nextNodes);
                return true;
            }
            return false;
        }

        HashSet<RunnableNode> FollowNode(Node currentNode, Node prevNode, RunnableNode lastRunnable)
        {
            // If this node has already been tried, it's path has already been followed - cancel this branch
            if (triedNodes.Contains(currentNode) && optimiseParallel)
                return new HashSet<RunnableNode>();
            // Node has been encountered - trigger the event
            currentNode.OnEncountered(prevNode, this);
            // The current chain's HashSet of runnables to stop at
            HashSet<RunnableNode> runnables = new HashSet<RunnableNode>();
            runnables.Add(lastRunnable);
            // The HashSet of nodes to test next
            HashSet<Node> nextNodes = new HashSet<Node>();
            // Get links from the current node to test next
            // Use node specified links if specified
            Link[] links = currentNode.NextLinks();
            if (links == null)
            {
                links = _model.GetAssociatedLinks(currentNode).ToArray();
            }
            // Store the node in the loop checking HashSet if doing so
            if (optimiseParallel)
                triedNodes.Add(currentNode);
            // If the current node is blocking, kill the chain
            if (currentNode.IsBlocking()) {
                return runnables;
            }
            // Test connected links
            foreach (Link link in links)
            {
                // Check the link is from this node, and not to
                if (currentNode == _model.GetNodeFromID(link._from))
                {
                    Node nextNode = _model.GetNodeFromID(link._to);
                    // Add the tested link to the current link chain for live preview
                    _currentLinks.Add(link);
                    nextNodes.Add(nextNode);
                }
            }
            currentNode.OnPassed(nextNodes, this);
            foreach (Node nextNode in nextNodes) {
                // If nextNode is a RunnableNode, store it as the next return point.
                // Otherwise continue with the last return point. 
                RunnableNode makeLastRunnable = lastRunnable;
                if (nextNode is RunnableNode)
                    makeLastRunnable = nextNode as RunnableNode;
                HashSet<RunnableNode> nextRunnables = FollowNode(nextNode, currentNode, makeLastRunnable);
                // If the model doesn't support parallel states, use first come first serve
                if (!_model.supportParallel)
                    return nextRunnables;
                else {
                    runnables.Remove(lastRunnable);
                    foreach (RunnableNode runnable in nextRunnables) {
                        runnables.Add(runnable);
                    }
                }
            }
            return runnables;
        }

        HashSet<RunnableNode> NodeHashSetsDiff (HashSet<RunnableNode> hashSet1, HashSet<RunnableNode> hashSet2) {
            HashSet<RunnableNode> differences = new HashSet<RunnableNode>();
            foreach (RunnableNode node in hashSet1) {
                if (!hashSet2.Contains(node))
                    differences.Add(node);
            }
            foreach (RunnableNode node in hashSet2) {
                if (!hashSet1.Contains(node))
                    differences.Add(node);
            }
            return differences;
        }

        void SetCurrentRunnables(HashSet<RunnableNode> runnables)
        {
            _currentRunnables = runnables;
        }

        public void SetModel(NodeMachineModel model)
        {
            this._model = model;
        }
        public NodeMachineModel GetModel()
        {
            return _model;
        }

    }

}