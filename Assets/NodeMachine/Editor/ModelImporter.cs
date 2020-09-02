using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

[ScriptedImporter(1, ".machine")]
public class ModelImporter : ScriptedImporter
{

    public override void OnImportAsset(AssetImportContext ctx)
    {
        NodeMachineModel modelData = ScriptableObject.CreateInstance<NodeMachineModel>();
        ctx.AddObjectToAsset("Main Machine", modelData);
        ctx.SetMainObject(modelData);
        modelData.LoadFromPath(ctx.assetPath);
    }

}
