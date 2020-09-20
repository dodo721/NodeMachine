using System;

namespace NodeMachine.States {

    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : Attribute
    {

        public EventAttribute() { }

    }

}