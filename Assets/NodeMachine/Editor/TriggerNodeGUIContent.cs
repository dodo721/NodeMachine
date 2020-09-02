using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;

[NodeGUI(typeof(TriggerNode))]
public class TriggerNodeGUIContent : NodeGUIContent {
    
    private bool editingName = false;

    public TriggerNodeGUIContent (TriggerNode node, NodeMachineEditor editor) : base (node, editor) {}

    public override bool DrawContent(Event e) {

        TriggerNode node = _node as TriggerNode;
        
        Rect content = new Rect();
        content.size = Transform.size;
        content.width -= 20;
        content.height -= 20;
        content.position = Transform.position;
        content.x += 10;
        content.y += 10;

        string prevName = node.name;

        GUILayout.BeginArea(content);
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        if (editingName) {
            node.name = EditorGUILayout.TextField(node.name);
            if (GUILayout.Button("Done")) {
                editingName = false;
            }
        } else {
            GUILayout.Label(node.name);
            if (GUILayout.Button("Edit name")) {
                editingName = true;
            }
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        return prevName != node.name;

    }

}