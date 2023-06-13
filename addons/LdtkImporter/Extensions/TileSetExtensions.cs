using Godot;

namespace LdtkImporter;

public static class TileSetExtensions
{
    public static void AddCustomDataLayerIfNotExist(this TileSet tileSet, string layerName, Variant.Type type)
    {
        var customDataLayerIndex = tileSet.GetCustomDataLayerByName(layerName);
        if (customDataLayerIndex != -1) return;

        tileSet.AddCustomDataLayer();
        
        customDataLayerIndex = tileSet.GetCustomDataLayersCount() - 1;
        
        tileSet.SetCustomDataLayerName(customDataLayerIndex, layerName);
        tileSet.SetCustomDataLayerType(customDataLayerIndex, type);
    }
}