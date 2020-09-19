using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Text.RegularExpressions;

[ScriptedImporter(1, ".machine")]
public class ModelImporter : ScriptedImporter
{

    public override void OnImportAsset(AssetImportContext ctx)
    {
        NodeMachineModel modelData = ScriptableObject.CreateInstance<NodeMachineModel>();
        modelData.filepath = ctx.assetPath;
        ctx.AddObjectToAsset("Main Machine", modelData);
        ctx.SetMainObject(modelData);
        string directory = GetDirectory(ctx.assetPath);
        if (!directory.Contains("Resources")) {
            Debug.LogWarning(modelData.name + ": NodeMachine Models MUST be placed in the Resources folder to work in game builds!", ctx.mainObject);
        }
        modelData.LoadFromPath();
    }

    static string GetDirectory (string filepath) {
        if (filepath.Length > 0)
        {
            if (!Directory.Exists(filepath))
            {
                filepath = Regex.Replace(filepath, "/[^/]*$", "/");
            }
        }
        return filepath;
    }

}
