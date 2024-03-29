using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;
using System;

[NodeGUI(typeof(RunFunctionNode))]
public class RunFunctionNodeGUIContent : NodeGUIContent {

    public RunFunctionNodeGUIContent (RunFunctionNode node, NodeMachineEditor editor) : base (node, editor) {}

    public override bool DrawContent(Event e) {

        RunFunctionNode node = _node as RunFunctionNode;
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

        GUILayout.Label("RUN FUNC", smallText);
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
        
        bool continuous = GUILayout.Toggle(node.continuous, " Loop");
        if (continuous != node.continuous) {
            node.continuous = continuous;
            modelNeedsSaving = true;
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        return modelNeedsSaving;

    }

}