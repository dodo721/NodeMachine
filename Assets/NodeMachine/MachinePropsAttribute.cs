using System;

namespace NodeMachine {

    [AttributeUsage(AttributeTargets.Class)]
    public class MachinePropsAttribute : Attribute
    {
        public string Model;

        public MachinePropsAttribute() { }

        public MachinePropsAttribute(string model)
        {
            this.Model = model;
        }

    }

}