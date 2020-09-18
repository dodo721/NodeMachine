using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using NodeMachine.Nodes;
using NodeMachine.States;

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
        private Dictionary<StateNode, State> stateInstances = new Dictionary<StateNode, State>();
        private HashSet<ActiveNode> _activeNodes;
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
                _model.ReloadProperties();
                _activeNodes = new HashSet<ActiveNode>();
                foreach (Node node in _model.GetNodes())
                {
                    node.OnGameStart(this);
                    if (node is ActiveNode)
                        _activeNodes.Add(node as ActiveNode);
                }
                HashSet<RunnableNode> startRunnables = new HashSet<RunnableNode>();
                startRunnables.Add(_model.GetNodes<EntryNode>()[0] as RunnableNode);
                SetCurrentRunnables(startRunnables);
            }
        }

        void OnValidate () {
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
                }
            }
        }

        void Checkin()
        {
            // Run checkin for current runnables
            if (_currentRunnables == null) {
                Debug.LogError("Machine " + name + " has nothing to run!");
                return;
            }
            foreach (RunnableNode runnable in _currentRunnables) {
                runnable.Checkin(this);
            }
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
            HashSet<RunnableNode> testNodes = new HashSet<RunnableNode>(_currentRunnables);
            foreach (RunnableNode node in _activeNodes) {
                testNodes.Add(node);
            }
            foreach (RunnableNode node in testNodes) {
                HashSet<RunnableNode> branchedNodes = FollowNode(node, null, node);
                foreach (RunnableNode branchedNode in branchedNodes) {
                    nextNodes.Add(branchedNode);
                }
            }
            nextNodes.RemoveWhere(n => n is ActiveNode);
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
                // If nextNode is an EndNode, kill the chain
                if (nextNode is EndNode) {
                    return new HashSet<RunnableNode>();
                }
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