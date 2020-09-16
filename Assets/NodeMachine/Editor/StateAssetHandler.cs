using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using NodeMachine;

public class StateAssetHandler {

    [MenuItem("Assets/Create/State script")]
    public static void ShowStateScriptMethodPopup()
    {
        NewStatePopup popup = new NewStatePopup("Create State script", "Enter a name for the new State script.", "New state", "OK", "Cancel");
        popup.OnSubmit += (text, model) => CreateStateScript(text, GetActiveDirectory(), model);
        popup.ShowUtility();
    }

    static string CreateStateScript (string name, string filepath, NodeMachineModel model) {
        if (name == null || filepath == null)
            return null;

        if (model == null)
            if (!EditorUtility.DisplayDialog("No model", "No model was specified. You will need to target to the model yourself.", "OK", "Cancel"))
                return null;

        if (model._propertyType != null) {
            if (!EditorUtility.DisplayDialog("State script exists", "This model already has an associated state script. Creating a new state script will cause conflicts. Continue?", "Yes", "No"))
                return null;
        }

        string classname = Regex.Replace(name, "[^a-zA-Z0-9_]", "");
        int nameAddition = 0;
        string chosenFilepath = filepath + "/" + name + ".cs";
        while (File.Exists(filepath))
        {
            chosenFilepath = filepath + "/" + name + " (" + nameAddition + ").cs";
            nameAddition++;
        }

        string codeBase = File.ReadAllText(Application.dataPath + "/NodeMachine/Editor/StateScriptBase.txt");
        
        codeBase = codeBase.Replace("<name>", classname);
        if (model != null) {
            codeBase = codeBase.Replace("<model_name>", model.name);
            codeBase = codeBase.Replace("<warning_comment>", "");
        } else {
            codeBase = codeBase.Replace("<warning_comment>", "!!WARNING!! State script is not targeted to a model!");
        }

        File.WriteAllText(chosenFilepath, codeBase);
        AssetDatabase.Refresh();
        return classname;
    }

    static string GetActiveDirectory () {
        string filepath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (filepath.Length > 0)
        {
            if (!Directory.Exists(filepath))
            {
                filepath = Regex.Replace(filepath, "/.*$", "/");
            }
        }
        return filepath;
    }

    private class NewStatePopup : EditorWindow {

        public string text;
        public NodeMachineModel model;
        private string message;
        private string okBtn;
        private string cancelBtn;
        private bool focused = false;
        private bool submitting = false;

        public delegate void SubmitEvent (string text, NodeMachineModel model);
        public event SubmitEvent OnSubmit;

        public NewStatePopup (string title, string message, string defaultText, string ok, string cancel) {
            Vector2 size = new Vector2(250, 100);
            position = new UnityEngine.Rect(Screen.width / 2 - (size.x / 2), Screen.height / 2 - (size.y / 2), size.x, size.y);
            maxSize = size;
            minSize = size;
            titleContent = new GUIContent(title);
            this.message = message;
            this.okBtn = ok;
            this.cancelBtn = cancel;
            this.text = defaultText;
        }

        void OnGUI () {
            GUIStyle wordWrap = new GUIStyle();
            wordWrap.wordWrap = true;
            Vector2 padding = new Vector2(5, 5);
            GUILayout.BeginArea(new Rect(padding, position.size - padding - new Vector2(5,5)));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(message, wordWrap);
            
            GUI.SetNextControlName("state name text field");
            text = GUILayout.TextField(text);

            if (!focused) {
                EditorGUI.FocusTextInControl("state name text field");
                focused = true;
            }

            model = EditorGUILayout.ObjectField(model, typeof(NodeMachineModel), false) as NodeMachineModel;

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(submitting);

            if (GUILayout.Button(okBtn, GUILayout.ExpandWidth(false))) {
                Submit(text);
            }
            if (GUILayout.Button(cancelBtn, GUILayout.ExpandWidth(false))) {
                Submit(null);
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();

            if (submitting)
                Close();
        }

        void Submit (string text) {
            submitting = true;
            if (OnSubmit != null)
                OnSubmit.Invoke(text, model);
        }

    }

}