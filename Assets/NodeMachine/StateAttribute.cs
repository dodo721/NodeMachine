using System;

namespace NodeMachine.States {

    [AttributeUsage(AttributeTargets.Method)]
    public class StateAttribute : Attribute
    {
        public bool Visible = true;

        public StateAttribute() { }

        public StateAttribute(bool visible)
        {
            this.Visible = visible;
        }

    }

}