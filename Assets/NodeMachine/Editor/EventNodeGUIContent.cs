using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;

[NodeGUI(typeof(EventNode))]
public class EventNodeGUIContent : NodeGUIContent {

    public EventNodeGUIContent (EventNode node, NodeMachineEditor editor) : base (node, editor) {}

    public override bool DrawContent(Event e) {

        EventNode node = _node as EventNode;
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
        GUILayout.Label(node.Valid ? node.eventMethodName : "Event not found!", largeText);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        return false;
    }

}