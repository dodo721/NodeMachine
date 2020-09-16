using System;

namespace NodeMachine.States {

    [AttributeUsage(AttributeTargets.Method)]
    public class StateAttribute : Attribute
    {
        public bool Visible = true;
        public bool RunOnEncounter = false;

        public StateAttribute() { }

    }

}