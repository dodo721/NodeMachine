using System;

namespace NodeMachine.States {

    [AttributeUsage(AttributeTargets.Class)]
    public class StateTargetAttribute : Attribute
    {
        public string Model;

        public StateTargetAttribute() { }

        public StateTargetAttribute(string model)
        {
            this.Model = model;
        }

    }

}