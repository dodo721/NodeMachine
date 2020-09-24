using UnityEngine;
using System;

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo("Conditional/Else")]
    public class ElseNode : Node
    {

        public ElseNode(NodeMachineModel model, Vector2 position) : base(model)
        {
            background = "Assets/NodeMachine/Editor/Editor Resources/else-node.png";
            transform = new Rect(position.x - 25, position.y - 25, 50, 50);
        }

        public override string ToString()
        {
            return "Else Node " + ID;
        }

        public override string BeforeAddLink(Link link)
        {
            if (!(model.GetNodeFromID(link._from) is ConditionNode) && link._to == ID)
            {
                return "Else nodes can only receive links from Condition nodes.";
            }
            return null;
        }

    }

}