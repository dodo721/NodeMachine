using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using NodeMachine;
using NodeMachine.States;

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo("Event")]
    public class EventNode : Node, ISerializationCallbackReceiver
    {

        public string stateTypeName;
        public string eventMethodName;
        public Type stateType;
        private Dictionary<Machine, Action> eventMethods;
        public string normalBackground;

        public EventNode(Type state, string method, NodeMachineModel model, Vector2 position) : base(model)
        {
            stateType = state;
            stateTypeName = stateType.AssemblyQualifiedName;
            eventMethodName = method;
            transform = new Rect(position.x, position.y, 150, 75);
            background = "builtin skins/darkskin/images/node1.png";
            normalBackground = background;
            //activeBackground = "builtin skins/darkskin/images/node5.png";
        }

        public override string ToString()
        {
            string typeName = stateType != null ? stateType.ToString() : stateTypeName.Split(',')[0];
            return typeName + "." + eventMethodName;
        }

        public void SetValid(bool valid)
        {
            this.Valid = valid;
            background = valid ? normalBackground : "builtin skins/darkskin/images/node6.png";
        }

        public void OnAfterDeserialize()
        {
            stateType = Type.GetType(stateTypeName);
            SetValid(stateType != null);
            if (stateType != null)
            {
                if (stateType.BaseType != typeof(State))
                {
                    SetValid(false);
                } else {
                    MethodInfo method = stateType.GetMethod(eventMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null)
                        SetValid(false);
                    else if (method.GetCustomAttribute<EventAttribute>() == null)
                        SetValid(false);
                }
            }
        }

        public override void OnLoad () {
            if (!Valid)
            {
                string typeName = stateTypeName.Split(',')[0];
                model.PushError(ToString() + " event is missing!", "Event " + ToString() + " could not be found!\nCheck if you have deleted or renamed the State script/method.", this);
            }
        }

        public void OnBeforeSerialize()
        {
            if (stateType != null)
                stateTypeName = stateType.AssemblyQualifiedName;
        }

        public override bool CanCreateLinkFrom()
        {
            return true;
        }

        public override bool IsBlocking()
        {
            return !Valid;
        }

        public override void OnEncountered(Node prevNode, Machine machine)
        {
            if (!Valid) {
                Debug.LogError("Encountered an invalid state! Check the referenced class exists and extends State");
                return;
            }
            if (eventMethods.ContainsKey(machine))
                eventMethods[machine]();
        }

        public override void OnGameStart(Machine machine)
        {
            if (eventMethods == null)
                eventMethods = new Dictionary<Machine, Action>();

            if (Valid)
            {
                State state = machine.gameObject.GetComponent(stateType) as State;
                if (state == null)
                    state = machine.gameObject.AddComponent(stateType) as State;

                eventMethods.Add(machine, (Action) Delegate.CreateDelegate(typeof(Action), state, eventMethodName));
            }
            else
            {
                throw new Exception("Could not add Event for " + ToString() + " as it could not be found!");
            }
        }

        public static EventNode GetEventNodeFromMethod (NodeMachineModel model, Type type, string methodName) {
            foreach (EventNode stateNode in model.GetNodes<EventNode>()) {
                if (stateNode.stateType == type && stateNode.eventMethodName == methodName)
                    return stateNode;
            }
            return null;
        }

    }

}