using System;

namespace NodeMachine {

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeMachinePropertyAttribute : Attribute
    {
        public string modelName;

        public NodeMachinePropertyAttribute(string modelName)
        {
            this.modelName = modelName;
        }

    }

}