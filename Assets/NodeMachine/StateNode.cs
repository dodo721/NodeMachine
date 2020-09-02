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
        public Action stateMethod;
        public bool Valid { get; private set; } = true;
        public string normalBackground;

        public bool IsClassState {
            get {
                return stateMethodName == "" || stateMethodName == null;
            }
        }

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
            return IsClassState ? stateType.ToString().Replace("NodeMachine.States.", "") : stateType.ToString() + "." + stateMethodName;
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
                } else if (!IsClassState) {
                    if (stateType.GetMethod(stateMethodName, BindingFlags.Instance | BindingFlags.Public) == null) {
                        SetValid(false);
                    }
                }
            }
        }

        public override void OnLoad () {
            if (!Valid)
            {
                string typeName = stateTypeName.Split(',')[0];
                string stateName = typeName + (IsClassState ? "" : "." + stateMethodName);
                model.PushError(stateName + " state is missing!", "State " + stateName + " could not be found!\nCheck if you have deleted or renamed the State script/method.", this);
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

        public override void OnRunStart(Machine machine)
        {
            State state = machine.GetComponent(stateType) as State;
            state.running = true;
        }

        public override void OnRunEnd(Machine machine)
        {
            State state = machine.GetComponent(stateType) as State;
            state.running = false;
        }

        public override void OnGameStart(Machine machine)
        {
            if (Valid)
            {
                State state = machine.gameObject.GetComponent(stateType) as State;
                if (state == null)
                    state = machine.gameObject.AddComponent(stateType) as State;

                if (!IsClassState)
                    stateMethod = (Action) Delegate.CreateDelegate(typeof(Action), state, stateMethodName);
            }
            else
            {
                throw new Exception("Could not add State for " + ToString() + " as it could not be found!");
            }
        }

        public override void Checkin(Machine machine)
        {
            State state = machine.GetComponent(stateType) as State;
            if (state.running)
            {
                if (stateMethod != null) {
                    stateMethod.Invoke();
                } else {
                    state.Checkin();
                }
            }
        }

        public static StateNode GetStateNodeFromTypeName (NodeMachineModel model, string typeName) {
            foreach (StateNode stateNode in model.GetNodes<StateNode>()) {
                if (stateNode.stateTypeName == typeName)
                    return stateNode;
            }
            return null;
        }

        public static StateNode GetStateNodeFromType <T> (NodeMachineModel model) where T : State {
            Type type = typeof(T);
            return GetStateNodeFromType(model, type);
        }

        public static StateNode GetStateNodeFromType (NodeMachineModel model, Type type) {
            if (!typeof(State).IsAssignableFrom(type)) {
                throw new Exception("Can only retrieve State Nodes by Type with Types inheriting State!");
            }
            foreach (StateNode stateNode in model.GetNodes<StateNode>()) {
                if (stateNode.stateType == type)
                    return stateNode;
            }
            return null;
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