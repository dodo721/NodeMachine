using UnityEngine;
using System;
using NodeMachine.States;

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo(false)]
    public class EntryStateNode : StateNode
    {

        public EntryStateNode(NodeMachineModel model) : base(typeof(Entry), model, new Vector2(0, 0))
        {
            background = "builtin skins/darkskin/images/node4.png";
            normalBackground = background;
        }

        public override bool CanBeRemoved()
        {
            return false;
        }
    }

}