using System;

namespace NodeMachine.States {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class StateInfoAttribute : Attribute
    {
        public string Machine;
        public bool Visible = true;
        public bool UsesMethods;

        public StateInfoAttribute() { }

        public StateInfoAttribute(string stateMachine)
        {
            this.Machine = stateMachine;
        }

        public StateInfoAttribute(string stateMachine, bool visible)
        {
            this.Machine = stateMachine;
            this.Visible = visible;
        }

        public StateInfoAttribute(bool visible)
        {
            this.Visible = visible;
        }

    }

}