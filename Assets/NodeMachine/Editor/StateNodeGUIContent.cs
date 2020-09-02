using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;

[NodeGUI(typeof(StateNode))]
public class StateNodeGUIContent : NodeGUIContent {
    
    private bool editingName = false;

    public StateNodeGUIContent (StateNode node, NodeMachineEditor editor) : base (node, editor) {}

    public override bool DrawContent(Event e) {

        StateNode node = _node as StateNode;
        if (node.IsClassState && node.Valid) {
            text = node.stateType.ToString();
        } else {
            GUIStyle smallText = new GUIStyle();
            smallText.fontSize = 9;
            smallText.alignment = TextAnchor.MiddleCenter;
            GUIStyle largeText = new GUIStyle();
            largeText.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginArea(Transform);
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Label(node.Valid ? node.stateType.ToString() : node.stateTypeName.Split(',')[0] + (node.IsClassState ? "" : "." + node.stateMethodName), smallText);
            GUILayout.Label(node.Valid ? node.stateMethodName : "State not found!", largeText);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        return false;
    }

}