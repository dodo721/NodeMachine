using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using NodeMachine;
using NodeMachine.States;

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo("State")]
    public class StateNode : RunnableNode, ISerializationCallbackReceiver
    {

        public string stateTypeName;
        public string stateMethodName;
        public Type stateType;
        public int testInt = 0;
        private Dictionary<Machine, Action> stateMethods;
        public string normalBackground;

        /*
        public StateNode(Type state, NodeMachineModel model, Vector2 position) : base(model)
        {
            stateType = state;
            stateTypeName = stateType.AssemblyQualifiedName;
            stateMethodName = "";
            transform = new Rect(position.x, position.y, 150, 75);
            background = "builtin skins/darkskin/images/node1.png";
            normalBackground = background;
            //activeBackground = "builtin skins/darkskin/images/node5.png";
        }
        */

        public StateNode(Type state, string method, NodeMachineModel model, Vector2 position) : base(model)
        {
            stateType = state;
            stateTypeName = stateType.AssemblyQualifiedName;
            stateMethodName = method;
            transform = new Rect(position.x, position.y, 150, 75);
            background = "builtin skins/darkskin/images/node2.png";
            normalBackground = background;
            //activeBackground = "builtin skins/darkskin/images/node5.png";
        }

        public override string ToString()
        {
            string typeName = stateType != null ? stateType.ToString() : stateTypeName.Split(',')[0];
            return typeName + "." + stateMethodName;
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
                } else if (stateType.GetMethod(stateMethodName, BindingFlags.Instance | BindingFlags.Public) == null) {
                    SetValid(false);
                }
            }
        }

        public override void OnLoad () {
            if (!Valid)
            {
                string typeName = stateTypeName.Split(',')[0];
                model.PushError(ToString() + " state is missing!", "State " + ToString() + " could not be found!\nCheck if you have deleted or renamed the State script/method.", this);
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

        public override void OnAddLink(Link link)
        {
            /*
            if (GetLinksFrom().Count == 2) {
                link.Auto = false;
                link._lockAutoState = true;
            }
            */
        }

        public override bool IsBlocking()
        {
            return !Valid;
        }

        public override void OnEncountered(Node prevNode, Machine machine)
        {
            if (!Valid)
                Debug.LogError("Encountered an invalid state! Check the referenced class exists and extends State");
        }

        public override void OnGameStart(Machine machine)
        {
            if (stateMethods == null)
                stateMethods = new Dictionary<Machine, Action>();

            if (Valid)
            {
                State state = machine.gameObject.GetComponent(stateType) as State;
                if (state == null)
                    state = machine.gameObject.AddComponent(stateType) as State;

                stateMethods.Add(machine, (Action) Delegate.CreateDelegate(typeof(Action), state, stateMethodName));
            }
            else
            {
                throw new Exception("Could not add State for " + ToString() + " as it could not be found!");
            }
        }

        public override void Checkin(Machine machine)
        {
            if (stateMethods.ContainsKey(machine))
                stateMethods[machine]();
        }

        public static StateNode GetStateNodeFromMethod (NodeMachineModel model, Type type, string methodName) {
            foreach (StateNode stateNode in model.GetNodes<StateNode>()) {
                if (stateNode.stateType == type && stateNode.stateMethodName == methodName)
                    return stateNode;
            }
            return null;
        }

    }

}