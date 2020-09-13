using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;

[NodeGUI(typeof(EntryNode))]
public class EntryNodeGUIContent : NodeGUIContent {

    public EntryNodeGUIContent (EntryNode node, NodeMachineEditor editor) : base (node, editor) {
        text = "Entry";
    }

}