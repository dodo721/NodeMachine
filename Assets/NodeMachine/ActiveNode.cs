using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Active")]
    public class ActiveNode : RunnableNode {

        public ActiveNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(100, 75);
            background = "builtin skins/darkskin/images/node5.png";
        }

        public override void Checkin (Machine machine) {}

    }

}