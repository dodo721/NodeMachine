using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using NodeMachine.Nodes;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace NodeMachine {

    public class PropertyMenu
    {

        //private static HashSet<PropertyMenu> menus = new HashSet<PropertyMenu>();
        
        private NodeMachineEditor _editor;
        private UnityEngine.Object currentPropsObj = null;
        private Machine lastSelectedMachine = null;
        private Dictionary<UnityEngine.Object, NodeMachineModel> targetedProps = new Dictionary<UnityEngine.Object, NodeMachineModel>();

        /*
        private bool UncompiledChanges {
            get {return _editor.uncompiledChanges;}
            set {_editor.uncompiledChanges = value;}
        }
        private Vector2 _scrollPos = Vector2.zero;
        private string _newFloatName = null;
        private string _newIntName = null;
        private string _newBoolName = null;

        private Texture2D refreshIconTex = null;
        private Regex forceAlphanumeric = new Regex("^[a-zA-Z][a-zA-Z0-9]*$");

        */

        public PropertyMenu(NodeMachineEditor editor)
        {
            this._editor = editor;

            //PropertyMenu.menus.Add(this);
            //refreshIconTex = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/refresh-16x16.png") as Texture2D;
        }

        public bool DrawMenu (bool isPlaying, Machine machine) {
            if (_editor._model == null)
                return false;
            UnityEngine.Object propsObj = null;
            if (machine != null) {
                if (machine == lastSelectedMachine) {
                    propsObj = currentPropsObj;
                } else {
                    Component[] objs = machine.GetComponents<Component>();
                    foreach (UnityEngine.Object obj in objs) {
                        if (targetedProps.ContainsKey(obj)) {
                            if (targetedProps[obj] == _editor._model) {
                                propsObj = obj;
                                break;
                            }
                        } else {
                            MachinePropsAttribute attr = obj.GetType().GetCustomAttribute<MachinePropsAttribute>();
                            if (attr != null) {
                                if (attr.Model == _editor._model.name) {
                                    targetedProps.Add(obj, _editor._model);
                                    propsObj = obj;
                                    break;
                                }
                            }
                        }
                    }
                }
            } else {
                propsObj = _editor._model.machinePropertiesDelegates.Keys.First();
            }
            
            if (machine != null) {
                EditorGUILayout.LabelField(propsObj.name, EditorStyles.boldLabel);
                EditorGUILayout.Space();
            }
            EditorGUI.BeginDisabledGroup(machine == null);
            foreach (string fieldName in _editor._model.machinePropertiesDelegates[propsObj].Keys) {
                DrawProp(fieldName, _editor._model.machinePropertiesDelegates[propsObj][fieldName]);
            }
            EditorGUI.EndDisabledGroup();
            return false;
        }

        void DrawProp (string fieldName, NodeMachineModel.MachinePropertyFieldDelegates fieldDelegates) {
            Type fieldType = fieldDelegates.fieldType;
            object value = fieldDelegates.getter();
            // Test field types by cached types, not on value:
            // null values will yield no type and not display
            if (fieldType == typeof(int)) {
                int newVal = EditorGUILayout.IntField(fieldName, (int)value);
                if (newVal != (int)value)
                    fieldDelegates.setter(newVal);
            }
            if (fieldType == typeof(float)) {
                float newVal = EditorGUILayout.FloatField(fieldName, (float)value);
                if (newVal != (float)value)
                    fieldDelegates.setter(newVal);
            }
            if (fieldType == typeof(bool)) {
                bool newVal = EditorGUILayout.Toggle(fieldName, (bool)value);
                if (newVal != (bool)value)
                    fieldDelegates.setter(newVal);
            }
            if (fieldType == typeof(GameObject)) {
                GameObject newVal = (GameObject)EditorGUILayout.ObjectField(fieldName, (UnityEngine.Object)value, typeof(GameObject), true);
                if (newVal != (GameObject)value)
                    fieldDelegates.setter(newVal);
            }
            if (fieldType == typeof(string)) {
                string newVal = EditorGUILayout.TextField(fieldName, (string)value);
                if (newVal != (string)value)
                    fieldDelegates.setter(newVal);
            }
        }
    }

}