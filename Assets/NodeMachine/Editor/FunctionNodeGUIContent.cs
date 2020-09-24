using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;

[NodeGUI(typeof(FunctionNode))]
public class FunctionNodeGUIContent : NodeGUIContent {

    public FunctionNodeGUIContent (FunctionNode node, NodeMachineEditor editor) : base (node, editor) {}

    public override bool DrawContent(Event e) {

        FunctionNode node = _node as FunctionNode;
        
        Rect content = new Rect();
        content.size = Transform.size;
        content.width -= 20;
        content.height -= 20;
        content.position = Transform.position;
        content.x += 10;
        content.y += 10;

        GUIStyle center = new GUIStyle();
        center.alignment = TextAnchor.MiddleCenter;
        center.normal.textColor = Color.white;
        GUIStyle smallText = new GUIStyle();
        smallText.fontSize = 9;
        smallText.alignment = TextAnchor.MiddleCenter;
        smallText.normal.textColor = Color.white;

        string prevName = node.name;

        GUILayout.BeginArea(content);
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.Label("Function", smallText);
        if (node.editingName) {
            node.name = EditorGUILayout.TextField(node.name);
        } else {
            GUILayout.Label(node.name, center);
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        return prevName != node.name;

    }

}