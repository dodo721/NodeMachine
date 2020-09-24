using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;
using System;

[NodeGUI(typeof(GotoFunctionNode))]
public class GotoFunctionNodeGUIContent : NodeGUIContent {

    public GotoFunctionNodeGUIContent (GotoFunctionNode node, NodeMachineEditor editor) : base (node, editor) {}

    public override bool DrawContent(Event e) {

        GotoFunctionNode node = _node as GotoFunctionNode;
        bool modelNeedsSaving = false;

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
        smallText.normal.textColor = Color.black;

        GUILayout.BeginArea(content);
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.Label("GOTO", smallText);
        Node[] functions = _editor._model.GetNodes<FunctionNode>();
        string[] functionNames = new string[functions.Length];
        for (int i = 0; i < functions.Length; i++) {
            functionNames[i] = (functions[i] as FunctionNode).name;
        }
        int selected = Array.IndexOf(functionNames, node.function);
        if (selected == -1)
            selected = 0;
        
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