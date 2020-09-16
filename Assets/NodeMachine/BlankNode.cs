using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Blank")]
    public class BlankNode : RunnableNode {

        public BlankNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(100, 75);
            background = "builtin skins/darkskin/images/node3.png";
        }

        public override void Checkin (Machine machine) {}

    }

}