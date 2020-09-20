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
            HashSet<Type> types = LoadStateTypes(model);
            HashSet<NodeMenuItem> menuItems = new HashSet<NodeMenuItem>();
            int stateCount = 0;
            foreach (Type type in types)
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                    StateAttribute methodStateInfo = method.GetCustomAttribute<StateAttribute>();
                    if (methodStateInfo != null) {
                        if (!methodStateInfo.Visible)
                            continue;
                        stateCount++;
                        bool runOnEncounter = methodStateInfo.RunOnEncounter;
                        menuItems.Add(new NodeMenuItem("States/" + type.ToString() + "/" + method.Name, () => {
                            StateNode node = new StateNode(type, method.Name, model, mousePosition);
                            node.runOnEncounter = runOnEncounter;
                            editor.AddNode(node);
                        } , false, false));
                    }
                }
            }
            if (stateCount == 0)
                menuItems.Add(new NodeMenuItem("States" , null, false, true));
            return menuItems.ToArray();
        }

        public NodeMenuItem[] NodeContextMenuItems(Node node, NodeMachineModel model)
        {
            return null;
        }

        public static HashSet<Type> LoadStateTypes(NodeMachineModel model)
        {
            Assembly assembly = Assembly.Load("Assembly-CSharp");
            IEnumerable<Type> stateTypes = assembly.GetTypes().Where(t => typeof(State).IsAssignableFrom(t));
            HashSet<Type> types = new HashSet<Type>();
            foreach (Type type in stateTypes)
            {
                if (type == typeof(State))
                    continue;
                StateTargetAttribute stateAttribute = type.GetCustomAttribute<StateTargetAttribute>();
                if (stateAttribute == null)
                    continue;
                if (stateAttribute.Model == model.name) {
                    types.Add(type);
                }
            }
            return types;
        }

    }

}