using UnityEditor;

// Turns on Read/Write for every model imported anywhere under Assets/Packages, so any art pack
// dropped there can be voxelized by VoxelDestructible without changing each model's import settings.
public class ReadableArtModels : AssetPostprocessor
{
    private const string ArtFolder = "Assets/Packages/";

    // Raise this number after changing the rules below, so Unity reimports the models with them
    public override uint GetVersion() => 2;

    private void OnPreprocessModel()
    {
        if (!assetPath.StartsWith(ArtFolder)) return;
        ((ModelImporter)assetImporter).isReadable = true;
    }
}
