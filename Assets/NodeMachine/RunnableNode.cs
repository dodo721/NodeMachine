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

        protected RunnableNode(NodeMachineModel model, Vector2 position) : base(model, position) { }
        protected RunnableNode(NodeMachineModel model) : base(model) { }

        public abstract void Checkin(Machine machine);

        public virtual void OnRunStart(Machine machine) { }
        public virtual void OnRunEnd(Machine machine) { }

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

    }

}