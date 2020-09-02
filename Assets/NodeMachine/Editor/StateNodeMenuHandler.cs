using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeMachine;
using NodeMachine.States;

namespace NodeMachine.Nodes {

    [NodeMenu(typeof(StateNode))]
    public class StateNodeMenuHandler : INodeMenuHandler
    {

        public NodeMenuItem[] AddNodeMenuItems(NodeMachineModel model, Vector2 mousePosition, NodeMachineEditor editor)
        {
            Dictionary<Type, StateInfoAttribute> types = LoadStateTypes();
            HashSet<NodeMenuItem> menuItems = new HashSet<NodeMenuItem>();
            foreach (Type type in types.Keys)
            {
                bool fromClass = false;
                bool disabled = false;
                StateInfoAttribute stateInfo = types[type];
                if (stateInfo == null)
                    fromClass = true;
                else {
                    if (!stateInfo.Visible)
                        continue;
                    else if (!stateInfo.UsesMethods) {
                        fromClass = true;
                    }
                }
                if (fromClass) {
                    disabled = StateNode.GetStateNodeFromType(model, type) != null;
                    menuItems.Add(new NodeMenuItem("States/" + (stateInfo != null ? (stateInfo.Machine != null ? stateInfo.Machine + "/" : "") : "") + type.ToString(), () => {
                        StateNode node = new StateNode(type, model, mousePosition);
                        editor.AddNode(node);
                    } , false, disabled));
                } else {
                    foreach (MethodInfo method in type.GetMethods()) {
                        StateInfoAttribute methodStateInfo = method.GetCustomAttribute<StateInfoAttribute>();
                        if (methodStateInfo != null) {
                            disabled = StateNode.GetStateNodeFromMethod(model, type, method.Name) != null;
                            menuItems.Add(new NodeMenuItem("States/" + (stateInfo.Machine != null ? stateInfo.Machine + "/" : "") + type.ToString() + "/" + method.Name, () => {
                                StateNode node = new StateNode(type, method.Name, model, mousePosition);
                                editor.AddNode(node);
                            } , false, disabled));
                        }
                    }
                }
            }
            return menuItems.ToArray();
        }

        public NodeMenuItem[] NodeContextMenuItems(Node node, NodeMachineModel model)
        {
            return null;
        }

        Dictionary<Type, StateInfoAttribute> LoadStateTypes()
        {
            Assembly assembly = Assembly.Load("Assembly-CSharp");
            IEnumerable<Type> stateTypes = assembly.GetTypes().Where(t => typeof(State).IsAssignableFrom(t));
            Dictionary<Type, StateInfoAttribute> types = new Dictionary<Type, StateInfoAttribute>();
            foreach (Type type in stateTypes)
            {
                if (type == typeof(State))
                    continue;
                StateInfoAttribute stateAttribute = type.GetCustomAttribute<StateInfoAttribute>();
                types.Add(type, stateAttribute);
            }
            return types;
        }

    }

}