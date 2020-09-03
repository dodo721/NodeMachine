using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeMachine;

namespace NodeMachine.Nodes {

[NodeMenu(typeof(ConditionNode))]
    public class ConditionNodeMenuHandler : INodeMenuHandler
    {

        public NodeMenuItem[] AddNodeMenuItems(NodeMachineModel model, Vector2 mousePosition, NodeMachineEditor editor)
        {
            string[] propTypes = editor._propTypesAvailable;
            string[] allPropTypes = { "FLOAT", "INT", "BOOL" };
            NodeMenuItem[] menuItems = new NodeMenuItem[allPropTypes.Length];
            int i = 0;
            foreach (string propType in allPropTypes)
            {
                bool disabled = !propTypes.Contains(propType);
                menuItems[i] = new NodeMenuItem("Conditions/" + propType, () =>
                {
                    // TODO : CONDITIONS AND PROPERTIES WITH NO STANDARD TYPES???
                    /*Condition.ConditionType type = Condition.ConditionTypeFromString(propType);
                    Condition condition = new Condition(editor._properties.GetPropNamesForType(type)[0], type, Condition.Comparison.EQUAL, Condition.GetDefaultValue(type));
                    ConditionNode node = new ConditionNode(editor._model, condition, mousePosition);
                    editor.AddNode(node);*/
                }, false, disabled);
                i++;
            }
            return menuItems;
        }

        public NodeMenuItem[] NodeContextMenuItems(Node node, NodeMachineModel model)
        {
            ConditionNode conNode = node as ConditionNode;
            return new NodeMenuItem[] {
                    new NodeMenuItem("Collapse",() => conNode.Collapse(!conNode.collapsed), conNode.collapsed, false)
                };
        }

    }

}