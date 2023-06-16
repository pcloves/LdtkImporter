using System;
using System.Linq;
using Godot;

namespace LdtkImporter;

public static class TileMapExtensions
{
    public static int EnsureLayerExist(this TileMap tileMap, string layerName)
    {
        var layersCount = tileMap.GetLayersCount();
        var layer = Enumerable.Range(0, layersCount)
            .Where(layer => tileMap.GetLayerName(layer).Equals(layerName))
            .FirstOrDefault(-1);

        if (layer != -1) return layer;

        tileMap.AddLayer(-1);
        layer = tileMap.GetLayersCount() - 1;
        tileMap.SetLayerName(layer, layerName);

        return layer;
    }

    public static void ActionByLayerNamePrefix(this TileMap tileMap, string layerNamePrefix, Action<int> action)
    {
        var layersCount = tileMap.GetLayersCount();
        var layers = Enumerable.Range(0, layersCount)
            .Where(layer => tileMap.GetLayerName(layer).StartsWith(layerNamePrefix));

        foreach (var layer in layers) action(layer);
    }
}