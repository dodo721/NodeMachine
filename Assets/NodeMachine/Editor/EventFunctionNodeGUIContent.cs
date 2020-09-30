using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;
using System;

[NodeGUI(typeof(EventFunctionNode))]
public class EventFunctionNodeGUIContent : NodeGUIContent {

    public EventFunctionNodeGUIContent (EventFunctionNode node, NodeMachineEditor editor) : base (node, editor) {}

    public override bool DrawContent(Event e) {

        EventFunctionNode node = _node as EventFunctionNode;
        bool modelNeedsSaving = false;

        Rect content = new Rect();
        content.size = Transform.size;
        content.width -= 20;
        content.height -= 20;
        content.position = Transform.position;
        content.x += 10;
        content.y += 10;

        GUIStyle smallText = new GUIStyle();
        smallText.fontSize = 9;
        smallText.alignment = TextAnchor.MiddleCenter;
        smallText.normal.textColor = Color.black;

        GUILayout.BeginArea(content);
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.Label("EVENT FUNC", smallText);
        Node[] functions = _editor._model.GetNodes<FunctionNode>();
        string[] functionNames = new string[functions.Length];
        for (int i = 0; i < functions.Length; i++) {
            functionNames[i] = (functions[i] as FunctionNode).name;
        }
        int selected = Array.IndexOf(functionNames, node.function);
        
        int newSel = EditorGUILayout.Popup(selected, functionNames);

        if (newSel != selected) {
            node.function = functionNames[newSel];
            modelNeedsSaving = true;
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        return modelNeedsSaving;

    }

}