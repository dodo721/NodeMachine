using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using NodeMachine.Nodes;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace NodeMachine {

    public class PropertyMenu
    {

        private static HashSet<PropertyMenu> menus = new HashSet<PropertyMenu>();
        private NodeMachineEditor _editor;
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

        public PropertyMenu(NodeMachineEditor editor)
        {
            this._editor = editor;
            PropertyMenu.menus.Add(this);
            refreshIconTex = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/refresh-16x16.png") as Texture2D;
        }
        
        [DidReloadScripts]
        static void OnScriptReload () {
            foreach (PropertyMenu menu in menus) {
                if (menu._editor.recompiling) {
                    menu._editor.recompiling = false;
                    menu._editor.uncompiledChanges = false;
                }
            }
        }

        public bool DrawMenu(bool isPlaying, Machine machine)
        {
            
            if (_editor._model._propertyType == null)
                UncompiledChanges = true;
            else if (_editor._model._propertyType.ToString() != PropertyIO.GetFormattedName(_editor._model.name))
                UncompiledChanges = true;
            else if (_editor._properties.CompareSchema(_editor._lastCompiledProps) == true)
                UncompiledChanges = false;
            
            bool modelNeedsSaving = false;
            CachedProperties properties = _editor._properties;
            
            GUIStyle italic = new GUIStyle();
            italic.fontStyle = FontStyle.Italic;
            italic.normal.textColor = new Color(1f, 1f, 1f, 0.5f);

            GUILayout.Label(_editor._model.name + " properties" + (UncompiledChanges ? "*" : ""), UncompiledChanges ? EditorStyles.boldLabel : EditorStyles.label);
            GUILayout.Label("  Script name: " + (_editor._model._propertyType == null ? "Needs compiling" : _editor._model._propertyType.ToString()), italic);
            GUILayout.Label("  " + (UncompiledChanges ? "Uncompiled changes" : "Up to date"), italic);
            EditorGUI.BeginDisabledGroup(_editor.recompiling);
            if (GUILayout.Button(!_editor.recompiling ? "Recompile" : "Compiling...")) {
                _editor.SaveModel();
                _editor._lastCompiledProps = _editor._properties.DeepCopy();
                _editor.recompiling = PropertyIO.CompileProperties(_editor._model, _editor._properties);
            }
            EditorGUI.EndDisabledGroup();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // Disable if the game is playing and global properties are being edited,
            //   (allow prop values to be changed in runtime but not structure)
            // or if local properties are being edited and there are uncompiled changes.
            //   (don't risk editing values that may not exist)
            // OR if properties are recompiling
            EditorGUI.BeginDisabledGroup(((isPlaying && machine == null) || (machine != null && UncompiledChanges)) || _editor.recompiling);
            if (machine != null && UncompiledChanges) {
                GUILayout.Label("Recompile to unlock editing");
            }

            EditorGUILayout.Space();

            // FLOATS
            bool serializeFloats = false;
            string floatToChange = null;
            float floatChangeVal = 0;
            string floatToRemove = null;
            GUILayout.Label("Floats", EditorStyles.boldLabel);
            foreach (string key in properties._floats.Keys)
            {
                GUILayout.BeginHorizontal();
                if (machine == null) {
                    float temp = EditorGUILayout.FloatField(key, properties._floats[key]);
                    if (properties._floats[key] != temp)
                    {
                        floatChangeVal = temp;
                        floatToChange = key;
                    }
                } else {
                    if (machine.properties != null)
                        machine.properties.SetProp(key, EditorGUILayout.FloatField(key, !UncompiledChanges ? (float)machine.properties.GetProp(key) : 0));
                }
                GUILayout.EndHorizontal();

                if (machine == null) {
                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(UncompiledChanges);
                    if (GUILayout.Button(new GUIContent(refreshIconTex), GUILayout.ExpandWidth(false)))
                    {
                        if (EditorUtility.DisplayDialog("Update property", "Update this property for all objects? Any local values will be overriden!", "Yes", "No")) {
                            TriggerPropValChange(key, properties._floats[key]);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                    {
                        floatToRemove = key;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (machine == null) {
                GUILayout.BeginHorizontal();
                GUI.SetNextControlName("add float text field");
                string tempFloatName = EditorGUILayout.TextField(_newFloatName);
                if (tempFloatName != null) {
                    if (forceAlphanumeric.IsMatch(tempFloatName))
                        _newFloatName = tempFloatName;
                }
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)) && _newFloatName != null)
                {
                    if (!properties.ContainsProp(_newFloatName))
                    {
                        UncompiledChanges = true;
                        properties.AddFloat(_newFloatName, 0);
                        _newFloatName = "";
                        serializeFloats = true;
                        GUI.changed = true;
                        modelNeedsSaving = true;
                        EditorGUI.FocusTextInControl("add float text field");
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (floatToChange != null)
            {
                properties._floats[floatToChange] = floatChangeVal;
                serializeFloats = true;
            }
            if (floatToRemove != null)
            {
                UncompiledChanges = true;
                _editor.RemoveFloat(floatToRemove);
                serializeFloats = true;
            }
            if (serializeFloats)
            {
                //Undo.RecordObject(_editor._model, "change float property");
                //_editor._model.properties.SerializeFloats(properties._floats);
                modelNeedsSaving = true;
            }

            // INTS
            bool serializeInts = false;
            string intToChange = null;
            int intChangeVal = 0;
            string intToRemove = null;
            GUILayout.Label("Ints", EditorStyles.boldLabel);
            foreach (string key in properties._ints.Keys)
            {
                GUILayout.BeginHorizontal();
                if (machine == null) {
                    int temp = EditorGUILayout.IntField(key, properties._ints[key]);
                    if (properties._ints[key] != temp)
                    {
                        intChangeVal = temp;
                        intToChange = key;
                    }
                } else {
                    if (machine.properties != null)
                        machine.properties.SetProp(key, EditorGUILayout.IntField(key, !UncompiledChanges ? (int)machine.properties.GetProp(key) : 0));
                }
                GUILayout.EndHorizontal();
                
                if (machine == null) {
                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(UncompiledChanges);
                    if (GUILayout.Button(new GUIContent(refreshIconTex), GUILayout.ExpandWidth(false)))
                    {
                        if (EditorUtility.DisplayDialog("Update property", "Update this property for all objects? Any local values will be overriden!", "Yes", "No")) {
                            TriggerPropValChange(key, properties._ints[key]);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                    {
                        intToRemove = key;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (machine == null) {
                GUILayout.BeginHorizontal();
                GUI.SetNextControlName("add int text field");
                string tempIntName = EditorGUILayout.TextField(_newIntName);
                if (tempIntName != null)
                    if (forceAlphanumeric.IsMatch(tempIntName))
                        _newIntName = tempIntName;
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)) && _newIntName != null)
                {
                    if (!properties.ContainsProp(_newIntName))
                    {
                        UncompiledChanges = true;
                        properties.AddInt(_newIntName, 0);
                        _newIntName = "";
                        serializeInts = true;
                        GUI.changed = true;
                        modelNeedsSaving = true;
                        EditorGUI.FocusTextInControl("add int text field");
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (intToChange != null)
            {
                properties._ints[intToChange] = intChangeVal;
                serializeInts = true;
            }
            if (intToRemove != null)
            {
                UncompiledChanges = true;
                _editor.RemoveInt(intToRemove);
                serializeInts = true;
            }
            if (serializeInts)
            {
                //Undo.RecordObject(_editor._model, "changed int property");
                //_editor._model.properties.SerializeInts(properties._ints);
                modelNeedsSaving = true;
            }

            // BOOLS
            bool serializeBools = false;
            string boolToChange = null;
            bool boolChangeVal = false;
            string boolToRemove = null;
            GUILayout.Label("Bools", EditorStyles.boldLabel);
            foreach (string key in properties._bools.Keys)
            {
                GUILayout.BeginHorizontal();
                if (machine == null) {
                    bool temp = EditorGUILayout.Toggle(key, properties._bools[key]);
                    if (properties._bools[key] != temp)
                    {
                        boolChangeVal = temp;
                        boolToChange = key;
                    }
                } else {
                    if (machine.properties != null)
                        machine.properties.SetProp(key, EditorGUILayout.Toggle(key, !UncompiledChanges ? (bool)machine.properties.GetProp(key) : false));
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                if (machine == null) {
                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(UncompiledChanges);
                    if (GUILayout.Button(new GUIContent(refreshIconTex), GUILayout.ExpandWidth(false)))
                    {
                        if (EditorUtility.DisplayDialog("Update property", "Update this property for all objects? Any local values will be overriden!", "Yes", "No")) {
                            TriggerPropValChange(key, properties._bools[key]);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                    {
                        boolToRemove = key;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            
            if (machine == null) {
                GUILayout.BeginHorizontal();
                GUI.SetNextControlName("add bool text field");
                string tempBoolName = EditorGUILayout.TextField(_newBoolName);
                if (tempBoolName != null)
                    if (forceAlphanumeric.IsMatch(tempBoolName))
                        _newBoolName = tempBoolName;
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)) && _newBoolName != null)
                {
                    if (!properties.ContainsProp(_newBoolName))
                    {
                        UncompiledChanges = true;
                        properties.AddBool(_newBoolName, false);
                        _newBoolName = "";
                        serializeBools = true;
                        GUI.changed = true;
                        modelNeedsSaving = true;
                        EditorGUI.FocusTextInControl("add bool text field");
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (boolToChange != null)
            {
                properties._bools[boolToChange] = boolChangeVal;
                serializeBools = true;
            }
            if (boolToRemove != null)
            {
                UncompiledChanges = true;
                _editor.RemoveBool(boolToRemove);
                serializeBools = true;
            }
            if (serializeBools)
            {
                //Undo.RecordObject(_editor._model, "changed bool property");
                //_editor._model.properties.SerializeBools(properties._bools);
                modelNeedsSaving = true;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();

            return modelNeedsSaving;
        }

        void TriggerPropValChange (string key, dynamic value) {
            _editor.SaveModel();
            _editor._model.TriggerPropValueChangeEvent(key, value);
        }
    }

}