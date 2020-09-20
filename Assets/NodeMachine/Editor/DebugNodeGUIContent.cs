using NodeMachine;
using NodeMachine.Nodes;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

[NodeGUI(typeof(DebugNode))]
public class DebugNodeGUIContent : NodeGUIContent {
    
    string[] fieldNames;

    public DebugNodeGUIContent (DebugNode node, NodeMachineEditor editor) : base (node, editor) {}

    void CacheFieldNames () {
        Dictionary<string, Type> template = _editor._model.machinePropsSchema;
        fieldNames = new string[template.Count + 1];
        fieldNames[0] = "-message-";
        if (template.Count == 0)
            return;
        int i = 1;
        foreach (string fieldName in template.Keys) {
            fieldNames[i] = fieldName;
            i++;
        }
    }

    public override bool DrawContent(Event e) {

        DebugNode node = _node as DebugNode;
        bool modelNeedsSaving = false;

        if (fieldNames == null)
            CacheFieldNames();
        
        GUIStyle labelBlack = new GUIStyle();
        labelBlack.normal.textColor = Color.black;

        Rect content = new Rect();
        content.size = Transform.size;
        content.width -= 20;
        content.height -= 20;
        content.position = Transform.position;
        content.x += 10;
        content.y += 10;

        int selectedProp = Array.IndexOf(fieldNames, node.propName);
        if (selectedProp == -1) {
            selectedProp = 0;
            node.message = "Prop " + node.propName + " missing for debug node!";
        }

        GUILayout.BeginArea(content);
        GUILayout.BeginVertical();
        GUILayout.Label("Debug Node", labelBlack);
        GUILayout.FlexibleSpace();

        int newProp = EditorGUILayout.Popup(selectedProp, fieldNames);
        if (newProp != selectedProp) {
            node.propName = fieldNames[newProp];
            node.logProp = newProp != 0;
            modelNeedsSaving = true;
        }

        if (newProp == 0) {
            string newMsg = EditorGUILayout.TextField(node.message);
            if (newMsg != node.message) {
                node.message = newMsg;
                modelNeedsSaving = true;
            }
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        return modelNeedsSaving;

    }

}