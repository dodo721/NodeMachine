using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Debug")]
    public class DebugNode : Node {

        public string message = "Debug node";
        public string propName;
        public bool logProp = false;

        public DebugNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(200, 100);
            background = "Assets/NodeMachine/Editor/Editor Resources/debug-node.png";
        }

        public override void OnEncountered (Node prevNode, Machine machine, NodeFollower context) {
            if (!logProp) {
                Debug.Log(message);
            } else {
                try {
                    Debug.Log(machine.name + " > " + propName + ": " + machine.GetProp(propName));
                } catch (KeyNotFoundException) {
                    Debug.LogError("Could not debug " + machine.name + " > " + propName + " - prop not found!");
                }
            }
        }

    }

}