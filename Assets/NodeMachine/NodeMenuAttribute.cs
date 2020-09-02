using System;

namespace NodeMachine.Nodes {

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeMenuAttribute : Attribute
    {
        public Type NodeType;

        public NodeMenuAttribute(Type NodeType)
        {
            this.NodeType = NodeType;
        }

    }

}