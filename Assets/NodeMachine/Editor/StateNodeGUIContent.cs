using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;

[NodeGUI(typeof(StateNode))]
public class StateNodeGUIContent : NodeGUIContent {

    public StateNodeGUIContent (StateNode node, NodeMachineEditor editor) : base (node, editor) {}

    public override bool DrawContent(Event e) {

        StateNode node = _node as StateNode;
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
        GUILayout.Label(node.Valid ? node.stateType.ToString() : node.ToString(), smallText);
        GUILayout.Label(node.Valid ? node.stateMethodName : "State not found!", largeText);
        if (node.runOnEncounter)
            GUILayout.Label("Run on encounter", smallText);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        return false;
    }

}