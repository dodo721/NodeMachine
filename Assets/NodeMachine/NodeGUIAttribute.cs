using System;

namespace NodeMachine.Nodes {

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeGUIAttribute : Attribute
    {
        public Type NodeType;

        public NodeGUIAttribute(Type NodeType)
        {
            this.NodeType = NodeType;
        }

    }

}