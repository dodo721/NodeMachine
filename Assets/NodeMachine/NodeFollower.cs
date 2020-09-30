using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using NodeMachine.Nodes;
using System;
using System.Collections.Generic;

namespace NodeMachine {
    public class NodeFollower {

        private NodeMachineModel _model;
        private Machine _machine;
        public NodeFollower Parent {
            get; private set;
        } = null;
        public HashSet<NodeFollower> Children {
            get; private set;
        } = new HashSet<NodeFollower>();
        private bool optimiseParallel = true;
        private Node startNode;
        private bool started = false;
        private HashSet<RunnableNode> _currentRunnables;
        private HashSet<Link> _currentLinks = new HashSet<Link>();
        
        public struct CurrentFollowState {
            public HashSet<RunnableNode> runnables;
            public HashSet<Link> links;
        }

        private string name;
        public bool restartable = false;
        public bool Active {
            get; private set;
        } = true;
        private bool encounteredEnd = false;

        public class NodePath {
            public NodePath fromPath;
            public Node currentNode;
            public NodePath toPath;
        }
        public bool recordNodePaths = false;
        public HashSet<NodePath> checkinNodePaths;

        public NodeFollower (Machine machine, Node startNode, NodeFollower parent) {
            this._machine = machine;
            this._model = machine._model;
            this.name = machine.name;
            this.startNode = startNode;
            this.Parent = parent;
            if (Parent != null) {
                parent.Children.Add(this);
            }
        }

        public NodeFollower (Machine machine, Node startNode, NodeFollower parent, bool restartable) {
            this._machine = machine;
            this._model = machine._model;
            this.name = machine.name;
            this.startNode = startNode;
            this.restartable = restartable;
            this.Parent = parent;
            if (Parent != null) {
                parent.Children.Add(this);
            }
        }

        public CurrentFollowState? Checkin()
        {
            if (_machine.propsObject == null)
                return null;
            // Run checkin for current runnables
            if (_currentRunnables == null)
                _currentRunnables = new HashSet<RunnableNode>();

            foreach (RunnableNode runnable in _currentRunnables) {
                runnable.Checkin(_machine, this);
            }
            // Test the model for the next nodes
            DoNodeFollow();
            
            // Check if the follower has ended
            if (_currentRunnables.Count == 0 || encounteredEnd) {
                if (restartable) {
                    started = false;
                } else {
                    FinishFollower();
                }
            }
            CurrentFollowState state;
            state.runnables = _currentRunnables;
            state.links = _currentLinks;
            return state;
        }

        /// <summary>
        ///  Begins a branched follow through the node network.
        ///  Returns true if new RunnableNodes were chosen.
        /// </summary>
        bool DoNodeFollow()
        {
            _currentLinks.Clear();
            encounteredEnd = false;
            HashSet<RunnableNode> nextNodes = new HashSet<RunnableNode>();
            HashSet<Node> testNodes = null;
            if (started) {
                testNodes = new HashSet<Node>(_currentRunnables);
            } else {
                testNodes = new HashSet<Node>();
                testNodes.Add(startNode);
                started = true;
            }
            
            /*
            // Set up node paths
            Dictionary<RunnableNode, NodePath> paths = null;
            if (checkinNodePaths == null)
                checkinNodePaths = new HashSet<NodePath>();
            if (recordNodePaths) {
                paths = new Dictionary<RunnableNode, NodePath>();
                foreach (RunnableNode node in testNodes) {
                    NodePath nodePath = new NodePath();
                    foreach (NodePath path in checkinNodePaths) {
                        if (path.fromPath.currentNode == node) {
                            nodePath.fromPath = path;
                            break;
                        }
                    }
                    nodePath.currentNode = node;
                    checkinNodePaths.Add(nodePath);
                    paths.Add(node, nodePath);
                }
            }
            checkinNodePaths.Clear();*/

            // Run node follows
            foreach (Node node in testNodes) {

                HashSet<RunnableNode> branchedNodes;
                branchedNodes = FollowNode(node, null, node is RunnableNode ? node as RunnableNode : null, new HashSet<Node>(), null);

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
                        node.OnRunStart(_machine, this);
                    } else if (!nextNodes.Contains(node)) {
                        node.OnRunEnd(_machine, this);
                    } else
                        Debug.LogWarning("Node " + node + " marked as changed but not changed in current nodes or new nodes!");
                }
                UpdateCurrents(nextNodes, _currentLinks);
                return true;
            }
            return false;
        }

        HashSet<RunnableNode> FollowNode(Node currentNode, Node prevNode, RunnableNode lastRunnable, HashSet<Node> triedNodes, NodePath path)
        {
            // If this node has already been tried, it's path has already been followed - cancel this branch
            if (triedNodes.Contains(currentNode) && optimiseParallel) {
                if (recordNodePaths)
                    checkinNodePaths.Add(path);
                return new HashSet<RunnableNode>();
            }
            /*if (triedNodes.Contains(currentNode) && optimiseParallel) {
                HashSet<RunnableNode> retNodes = new HashSet<RunnableNode>();
                retNodes.Add(lastRunnable);
                return retNodes;
            }*/
            // Node has been encountered - trigger the event
            currentNode.OnEncountered(prevNode, _machine, this);
            // The current chain's HashSet of runnables to stop at
            HashSet<RunnableNode> runnables = new HashSet<RunnableNode>();
            runnables.Add(lastRunnable);
            // The HashSet of nodes to test next
            HashSet<Node> nextNodes = new HashSet<Node>();
            // Get nodes from the current node to test next
            // If not specified, default to link testing
            Node[] givenNextNodes = currentNode.NextNodes();
            if (givenNextNodes != null) {
                foreach (Node node in givenNextNodes) {
                    if (node == null)
                        Debug.LogError("Given null node to follow from " + currentNode + "!");
                    else
                        nextNodes.Add(node);
                }
            } else {
                // Get links from the current node to test next
                // Use node specified links if specified
                Link[] links = currentNode.NextLinks();
                if (links == null)
                {
                    links = _model.GetOutputLinks(currentNode).ToArray();
                }
                // Store the node in the loop checking HashSet if doing so
                if (optimiseParallel)
                    triedNodes.Add(currentNode);
                // If the current node is blocking, kill the chain
                if (currentNode.IsBlocking()) {
                    if (recordNodePaths)
                        checkinNodePaths.Add(path);
                    return runnables;
                }
                // Test connected links
                foreach (Link link in links)
                {
                    Node nextNode = _model.GetNodeFromID(link._to);
                    // Add the tested link to the current link chain for live preview
                    _currentLinks.Add(link);
                    nextNodes.Add(nextNode);
                }
            }
            currentNode.OnPassed(nextNodes, _machine, this);
            foreach (Node nextNode in nextNodes) {
                // Record next node as an entry in the path
                NodePath newPath = null;
                if (recordNodePaths) {
                    newPath = new NodePath();
                    newPath.currentNode = nextNode;
                    newPath.fromPath = path;
                    path.toPath = newPath;
                }
                // If nextNode is an EndNode, kill the chain
                if (nextNode is EndNode) {
                    if (recordNodePaths)
                        checkinNodePaths.Add(newPath);
                    encounteredEnd = true;
                    return new HashSet<RunnableNode>();
                }
                // If nextNode is a RunnableNode, store it as the next return point.
                // Otherwise continue with the last return point.
                RunnableNode makeLastRunnable = lastRunnable;
                if (nextNode is RunnableNode)
                    makeLastRunnable = nextNode as RunnableNode;
                // Set up new triedNodes listing
                HashSet<Node> newTriedNodes = new HashSet<Node>(triedNodes);
                HashSet<RunnableNode> nextRunnables = FollowNode(nextNode, currentNode, makeLastRunnable, newTriedNodes, newPath);
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
            runnables.RemoveWhere(r => r == null);
            // Check if there are any runnables different from the last one given for this chain.
            bool runnableChange = false;
            foreach (RunnableNode runnable in runnables) {
                if (runnable != lastRunnable) {
                    runnableChange = true;
                    break;
                }
            }
            // If there is a change, remove the last runnable, otherwise maintain the chain.
            if (runnableChange) {
                runnables.Remove(lastRunnable);
            }
            if (recordNodePaths)
                checkinNodePaths.Add(path);
            return runnables;
        }

        HashSet<RunnableNode> NodeHashSetsDiff (HashSet<RunnableNode> hashSet1, HashSet<RunnableNode> hashSet2) {
            HashSet<RunnableNode> differences = new HashSet<RunnableNode>();
            if (hashSet1 == null)
                return hashSet2;
            else if (hashSet2 == null)
                return hashSet1;
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

        void UpdateCurrents(HashSet<RunnableNode> runnables, HashSet<Link> links)
        {
            _currentRunnables = runnables;
            _currentLinks = links;
        }

        public void FinishFollower () {
            Debug.Log("Finishing follower " + this + " from " + startNode + " with parent " + Parent);
            Active = false;
            if (Parent != null) {
                Parent.FinishChild(this);
            } else {
                _machine.FinishFollower(startNode);
            }
        }

        public void FinishChild (NodeFollower child) {
            Children.Remove(child);
        }

    }
}