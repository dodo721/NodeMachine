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

    [NodeMenu(typeof(EventNode))]
    public class EventNodeMenuHandler : INodeMenuHandler
    {

        public NodeMenuItem[] AddNodeMenuItems(NodeMachineModel model, Vector2 mousePosition, NodeMachineEditor editor)
        {
            HashSet<Type> types = StateNodeMenuHandler.LoadStateTypes(model);
            HashSet<NodeMenuItem> menuItems = new HashSet<NodeMenuItem>();
            int eventCount = 0;
            foreach (Type type in types)
            {
                foreach (MethodInfo method in type.GetMethods()) {
                    EventAttribute methodEventInfo = method.GetCustomAttribute<EventAttribute>();
                    if (methodEventInfo != null) {
                        eventCount++;
                        menuItems.Add(new NodeMenuItem("Events/" + type.ToString() + "/" + method.Name, () => {
                            EventNode node = new EventNode(type, method.Name, model, mousePosition);
                            editor.AddNode(node);
                        } , false, false));
                    }
                }
            }
            if (eventCount == 0)
                menuItems.Add(new NodeMenuItem("Events" , null, false, true));
            return menuItems.ToArray();
        }

        public NodeMenuItem[] NodeContextMenuItems(Node node, NodeMachineModel model)
        {
            return null;
        }

    }

}