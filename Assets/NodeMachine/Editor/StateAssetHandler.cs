using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using NodeMachine;

public class StateAssetHandler {

    [MenuItem("Assets/Create/State script/Method based")]
    public static void ShowStateScriptMethodPopup()
    {
        NewStatePopup popup = new NewStatePopup("Create State script", "Enter a name for the new State script.", "New state", "OK", "Cancel");
        popup.OnSubmit += (text, model) => CreateStateScript(text, GetActiveDirectory(), model, true);
        popup.ShowUtility();
    }

    [MenuItem("Assets/Create/State script/Class based")]
    public static void ShowStateScriptClassPopup()
    {
        NewStatePopup popup = new NewStatePopup("Create State script", "Enter a name for the new State.", "New state", "OK", "Cancel");
        popup.OnSubmit += (text, model) => CreateStateScript(text, GetActiveDirectory(), model, false);
        popup.ShowUtility();
    }

    static void CreateStateScript (string name, string filepath, NodeMachineModel model, bool usingMethods) {
        if (name == null || filepath == null)
            return;
        
        if (model == null)
            if (!EditorUtility.DisplayDialog("No model", "No model was specified. You will need to set up your model properties in the State code yourself.", "OK", "Cancel"))
                return;
        
        if (model._propertyType == null) {
            EditorUtility.DisplayDialog("Uncompiled properties", "The model has uncompiled properties. Compile them from the NodeMachine editor before creating states.", "OK");
            return;
        }

        string classname = Regex.Replace(name, "[^a-zA-Z0-9_]", "");
        int nameAddition = 0;
        string chosenFilepath = filepath + "/" + name + ".cs";
        while (File.Exists(filepath))
        {
            chosenFilepath = filepath + "/" + name + " (" + nameAddition + ").cs";
            nameAddition++;
        }
        string codeBaseMethod = Application.dataPath + "/NodeMachine/Editor/StateMethodBase.txt";
        string codeBaseClass = Application.dataPath + "/NodeMachine/Editor/StateClassBase.txt";
        string codeBaseFile = usingMethods ? codeBaseMethod : codeBaseClass;
        string codeBase = File.ReadAllText(codeBaseFile);

        codeBase = codeBase.Replace("<name>", classname);
        
        string props = "";

        if (model != null) {
            if (model._propertyType != null) {
                props = "\t" + model._propertyType.ToString() + " props;\n\n";
                props += "\tvoid Start () {\n";
                props += "\t\tprops = properties as " + model._propertyType.ToString() + ";\n";
                props += "\t}";
            }
        }

        codeBase = codeBase.Replace("<props>", props);

        File.WriteAllText(chosenFilepath, codeBase);
        AssetDatabase.Refresh();
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