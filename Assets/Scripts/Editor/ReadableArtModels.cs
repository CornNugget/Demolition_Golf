using System.Collections.Generic;
using UnityEditor;

// Turns on Read/Write for the models VoxelDestructible may need to voxelize, so nobody has to change
// import settings by hand:
// - every model imported anywhere under Assets/Packages (art packs) or Assets/Objects3D
// - every model used by a prefab in Assets/Objects3D, wherever that model is stored
public class ReadableArtModels : AssetPostprocessor
{
    private static readonly string[] ModelFolders = { "Assets/Packages/", "Assets/Objects3D/" };
    private const string PrefabFolder = "Assets/Objects3D";

    // Raise this number after changing the rules below, so Unity reimports the models with them
    public override uint GetVersion() => 3;

    private void OnPreprocessModel()
    {
        foreach (string folder in ModelFolders)
        {
            if (assetPath.StartsWith(folder))
            {
                ((ModelImporter)assetImporter).isReadable = true;
                return;
            }
        }
    }

    // A prefab saved or moved into Objects3D can use a model from anywhere in the project
    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        var prefabs = new List<string>();
        foreach (string path in imported) if (IsObjects3DPrefab(path)) prefabs.Add(path);
        foreach (string path in moved) if (IsObjects3DPrefab(path)) prefabs.Add(path);

        // Changing import settings is not allowed while Unity is still importing, so wait a frame
        if (prefabs.Count > 0) EditorApplication.delayCall += () => MakeModelsReadable(prefabs.ToArray());
    }

    // Also checks the prefabs already in Objects3D whenever scripts reload; models that are readable are skipped
    [InitializeOnLoadMethod]
    private static void CheckExistingPrefabs()
    {
        EditorApplication.delayCall += () =>
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
            var paths = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++) paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            MakeModelsReadable(paths);
        };
    }

    private static bool IsObjects3DPrefab(string path) => path.StartsWith(PrefabFolder + "/") && path.EndsWith(".prefab");

    // Includes models used by prefabs nested inside these prefabs
    private static void MakeModelsReadable(string[] prefabPaths)
    {
        if (prefabPaths.Length == 0) return;
        foreach (string path in AssetDatabase.GetDependencies(prefabPaths, true))
        {
            if (AssetImporter.GetAtPath(path) is ModelImporter model && !model.isReadable)
            {
                model.isReadable = true;
                model.SaveAndReimport();
            }
        }
    }
}
