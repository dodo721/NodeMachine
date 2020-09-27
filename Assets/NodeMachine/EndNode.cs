using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("End")]
    public class EndNode : Node {

        public EndNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(100, 75);
            background = "builtin skins/darkskin/images/node6.png";
        }

        public override bool CanCreateLinkFrom () {
            return false;
        }

    }

}