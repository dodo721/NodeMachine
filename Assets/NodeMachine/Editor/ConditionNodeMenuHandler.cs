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
            NodeMenuItem menuItem;

            if (model.machinePropertiesDelegates.First().Value.Count > 0) {
                menuItem = new NodeMenuItem("Conditional/Condition", () =>
                {
                    // TODO : CONDITIONS AND PROPERTIES WITH NO STANDARD TYPES???
                    KeyValuePair<string, NodeMachineModel.MachinePropertyFieldDelegates> kvp = model.machinePropertiesDelegates.First().Value.First();
                    Condition.ConditionType? tryParseType = Condition.ParseConditionType(kvp.Value.fieldType);
                    if (tryParseType == null) {
                        EditorUtility.DisplayDialog("Error", "There was an error while creating the condition!", "OK");
                        return;
                    }
                    Condition.ConditionType fieldType = (Condition.ConditionType) tryParseType;
                    string fieldName = kvp.Key;
                    Condition condition = new Condition(fieldName, fieldType, Condition.Comparison.EQUAL, Condition.GetDefaultValue(fieldType));
                    ConditionNode node = new ConditionNode(editor._model, condition, mousePosition);
                    editor.AddNode(node);
                }, false, false);
            } else {
                menuItem = new NodeMenuItem("Condition", null, false, true);
            }

            NodeMenuItem[] menuItems = {menuItem};
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