using System;

namespace NodeMachine {

    [AttributeUsage(AttributeTargets.Field)]
    public class UsePropAttribute : Attribute
    {

        public UsePropAttribute() { }

    }

}