using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Functions/Goto Function")]
    public class GotoFunctionNode : Node {

        public string function = "function";

        public GotoFunctionNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(150, 75);
            background = "Assets/NodeMachine/Editor/Editor Resources/goto-node.png";
        }

        public override Node[] NextNodes () {
            FunctionNode funcNode = model.GetFunction(function);
            return new Node[] {funcNode};
        }

    }

}