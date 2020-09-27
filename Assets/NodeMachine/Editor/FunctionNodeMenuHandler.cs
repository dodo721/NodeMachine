using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeMachine;

namespace NodeMachine.Nodes {

[NodeMenu(typeof(FunctionNode))]
    public class FunctionNodeMenuHandler : INodeMenuHandler
    {

        public NodeMenuItem[] AddNodeMenuItems(NodeMachineModel model, Vector2 mousePosition, NodeMachineEditor editor)
        {
            return null;
        }

        public NodeMenuItem[] NodeContextMenuItems(Node node, NodeMachineModel model)
        {
            FunctionNode funcNode = node as FunctionNode;
            return new NodeMenuItem[] {
                new NodeMenuItem("Edit name",() => {
                    if (!funcNode.editingName)
                        funcNode.editingName = true;
                    else {
                        FunctionNode func = model.GetFunction(funcNode.name);
                        if (func != null && func != funcNode) {
                            EditorUtility.DisplayDialog("Function exists!", "A function with that name already exists.", "OK");
                            return;
                        }
                        funcNode.editingName = false;
                        model.UpdateFunctionCache();
                    }
                }, funcNode.editingName, false),
                new NodeMenuItem("Hide function group",() => {
                    foreach (Node hide in funcNode.GetFunctionGroup()) {
                        hide.visible = false;
                    }
                }, false, false),
                new NodeMenuItem("Reveal function group",() => {
                    foreach (Node hide in funcNode.GetFunctionGroup()) {
                        hide.visible = true;
                    }
                }, false, false),
                new NodeMenuItem("Remove function group",() => {
                    if (EditorUtility.DisplayDialog("Remove function group", "Are you sure you want to remove all nodes in this function?", "Yes", "No")) {
                        foreach (Node remove in funcNode.GetFunctionGroup()) {
                            model.RemoveNode(remove);
                        }
                    }
                }, false, false)
            };
        }

    }

}