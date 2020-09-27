using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Functions/Run Function")]
    public class RunFunctionNode : RunnableNode {

        public string function = "function";

        public RunFunctionNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(150, 75);
            background = "Assets/NodeMachine/Editor/Editor Resources/run-func-node.png";
        }

        public override void Checkin (Machine machine) {

        }

    }

}