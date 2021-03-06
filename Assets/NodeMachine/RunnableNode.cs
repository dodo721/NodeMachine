using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using SimpleJSON;

namespace NodeMachine.Nodes {

    /// <summary>
    ///  Represents a runnable node on a state machine.
    /// </summary>
    [Serializable]
    public abstract class RunnableNode : Node
    {
        public string activeBackground;
        public bool runOnEncounter = false;

        protected RunnableNode(NodeMachineModel model, Vector2 position) : base(model, position) { }
        protected RunnableNode(NodeMachineModel model) : base(model) { }

        public abstract void Checkin(Machine machine, NodeFollower context);

        public virtual void OnRunStart(Machine machine, NodeFollower context) { }
        public virtual void OnRunEnd(Machine machine, NodeFollower context) { }

        public bool ActivateTrigger (string name) {
            bool foundTriggersOfName = false;
            foreach (int id in linkIDs) {
                Link link = model.GetLinkFromID(id);
                Node node = model.GetNodeFromID(link._to);
                if (node is TriggerNode) {
                    TriggerNode triggerNode = node as TriggerNode;
                    if (triggerNode.name == name) {
                        foundTriggersOfName = true;
                        triggerNode.Trigger();
                    }
                }
            }
            return foundTriggersOfName;
        }

        public override void OnEncountered(Node prevNode, Machine machine, NodeFollower context) {
            if (runOnEncounter)
                Checkin(machine, context);
        }

    }

}