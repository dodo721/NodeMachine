using System;

namespace NodeMachine.Nodes {

    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public class NodeInfoAttribute : Attribute
    {
        public string menuLabel;
        public bool visible = true;

        public NodeInfoAttribute() { }
        public NodeInfoAttribute(string menuLabel)
        {
            this.menuLabel = menuLabel;
        }
        public NodeInfoAttribute(bool visible)
        {
            this.visible = visible;
        }
        public NodeInfoAttribute(string menuLabel, bool visible)
        {
            this.menuLabel = menuLabel;
            this.visible = visible;
        }

    }

}