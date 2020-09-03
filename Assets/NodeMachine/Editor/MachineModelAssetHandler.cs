using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Text.RegularExpressions;
using NodeMachine;

public class MachineModelAssetHandler
{
    [OnOpenAssetAttribute(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        string assetPath = AssetDatabase.GetAssetPath(instanceID);
        NodeMachineModel model = AssetDatabase.LoadAssetAtPath<NodeMachineModel>(assetPath);
        if (model != null)
        {
            model.ReloadModel();
            NodeMachineEditor window = (NodeMachineEditor)EditorWindow.GetWindow(typeof(NodeMachineEditor));
            window.LoadModel(model);
            NodeMachineEditor.ShowWindow(window);
            return true;
        }
        return false; //let unity open it.
    }

    [MenuItem("Assets/Create/NodeMachine Model")]
    public static void ShowNewModelPopup () {
        NewModelPopup popup = new NewModelPopup("New model", "Enter a name for the new model", "New model", "OK", "Cancel");
        popup.OnSubmit += CreateModelAsset;
        popup.ShowUtility();
    }

    static void CreateModelAsset(string name)
    {
        string filepath = GetActiveDirectory();
        string fileName = name + ".machine";
        string filepathCombined = filepath + "/" + fileName;
        int nameAddition = 0;
        while (File.Exists(filepathCombined))
        {
            fileName = "New machine (" + nameAddition + ").machine";
            filepathCombined = filepath + "/" + fileName;
            nameAddition++;
        }
        NodeMachineModel model = NodeMachineModel.SaveNewModel(filepathCombined);
    }

    static string CreateStateScript (string name, string filepath, NodeMachineModel model) {
        if (name == null || filepath == null)
            return null;
        
        if (model == null)
            if (!EditorUtility.DisplayDialog("No model", "No model was specified. You will need to set up your model properties in the State code yourself.", "OK", "Cancel"))
                return null;
        
        if (model._propertyType == null) {
            EditorUtility.DisplayDialog("Uncompiled properties", "The model has uncompiled properties. Compile them from the NodeMachine editor before creating states.", "OK");
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

    private class NewModelPopup : EditorWindow {

        public string text;
        private string message;
        private string okBtn;
        private string cancelBtn;
        private bool focused = false;
        private bool submitting = false;

        public delegate void SubmitEvent (string text);
        public event SubmitEvent OnSubmit;

        public NewModelPopup (string title, string message, string defaultText, string ok, string cancel) {
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
                OnSubmit.Invoke(text);
        }

    }

}
