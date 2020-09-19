using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo("Else")]
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

        public override bool BeforeAddLink(Link link)
        {
            if (!(model.GetNodeFromID(link._from) is ConditionNode) && link._to == ID)
            {
    #if UNITY_EDITOR
                EditorUtility.DisplayDialog("Unable to create link", "Else nodes can only receive links from Condition nodes.", "OK");
    #endif
                return false;
            }
            return true;
        }

    }

}