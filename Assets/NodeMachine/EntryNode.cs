using UnityEngine;
using System;
using NodeMachine.States;

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo(false)]
    public class EntryNode : RunnableNode
    {

        public EntryNode(NodeMachineModel model) : base(model)
        {
            transform = new Rect(0, 0, 150, 75);
            background = "builtin skins/darkskin/images/node4.png";
        }

        public override void Checkin(Machine machine) {}

        public override bool CanBeRemoved()
        {
            return false;
        }

        public override bool CanBeHidden()
        {
            return false;
        }

        public override string BeforeAddLink (Link link) {
            if (link._to == ID)
                return "Cannot create a link to the Entry node.";
            else
                return null;
        }
    }

}