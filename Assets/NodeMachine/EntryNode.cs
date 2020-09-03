using UnityEngine;
using System;
using NodeMachine.States;

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo(false)]
    public class EntryNode : RunnableNode
    {

        public EntryNode(NodeMachineModel model) : base(model, new Vector2(0, 0))
        {
            background = "builtin skins/darkskin/images/node4.png";
        }

        public override void Checkin(Machine machine) {}

        public override bool CanBeRemoved()
        {
            return false;
        }
    }

}