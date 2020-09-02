using UnityEditor;
using UnityEngine;
using System;

namespace NodeMachine.Nodes {

    public interface INodeMenuHandler
    {

        /// <summary>
        ///  Returns the list of menu items to add to the "Add node" menu of the State Machine editor.
        /// </summary>
        /// <param name="model">The Model being edited.</param>
        NodeMenuItem[] AddNodeMenuItems(NodeMachineModel model, Vector2 mousePosition, NodeMachineEditor editor);

        /// <summary>
        ///  Returns the list of menu items to add to the context menu of this node type.
        /// </summary>
        /// <param name="node">The selected node.</param>
        NodeMenuItem[] NodeContextMenuItems(Node node, NodeMachineModel model);

    }

    public struct NodeMenuItem
    {
        public string label;
        public GenericMenu.MenuFunction Function;
        public bool ticked;
        public bool disabled;

        public NodeMenuItem(string label, GenericMenu.MenuFunction Function, bool ticked, bool disabled)
        {
            this.label = label;
            this.Function = Function;
            this.ticked = ticked;
            this.disabled = disabled;
        }
    }

}