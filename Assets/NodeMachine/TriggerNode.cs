using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Trigger")]
    public class TriggerNode : Node {

        public string name = "default trigger";
        private bool triggered = false;

        public TriggerNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(100, 75);
            background = "Assets/NodeMachine/Editor/Editor Resources/trigger-node.png";
        }

        public override bool IsBlocking() {
            return !triggered;
        }

        public override void OnPassed(HashSet<Node> nextNodes, Machine machine, NodeFollower context) {
            triggered = false;
        }

        public void Trigger () {
            triggered = true;
        }

    }

}