using UnityEngine;
using UnityEditor;
using NodeMachine.Nodes;
using System;
using System.Collections.Generic;

namespace NodeMachine {

    public class ErrorPanel
    {

        private NodeMachineEditor _editor;
        private Vector2 _scrollPos = Vector2.zero;

        public ErrorPanel(NodeMachineEditor editor)
        {
            this._editor = editor;
        }

        public void Draw () {
            
            List<NodeError> errors = _editor._model.nodeErrors;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(150));
            
            if (errors.Count == 0) {

                GUIStyle italic = new GUIStyle();
                italic.fontStyle = FontStyle.Italic;
                italic.normal.textColor = new Color(1f, 1f, 1f, 0.5f);

                GUILayout.Label("  No errors!", italic);
            } else {

                GUIStyle errorStyle = new GUIStyle();
                errorStyle.normal.textColor = Color.red;

                foreach (NodeError error in errors) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(error.error, errorStyle);
                    if (GUILayout.Button(" ", GUILayout.ExpandWidth(false))) {
                        _editor._uncenteredOffset = -error.source.transform.position;
                    }
                    GUILayout.EndHorizontal();
                }

            }

            EditorGUILayout.EndScrollView();

        }

    }
}