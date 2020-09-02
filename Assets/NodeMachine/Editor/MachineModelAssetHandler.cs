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
    public static void CreateStateMachineAsset()
    {
        string filepath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (filepath.Length > 0)
        {
            if (!Directory.Exists(filepath))
            {
                filepath = Regex.Replace(filepath, "/.*$", "");
            }
        }
        string fileName = "New machine.machine";
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

}
