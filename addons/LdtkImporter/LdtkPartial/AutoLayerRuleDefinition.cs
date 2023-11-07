using System;
using System.Diagnostics.CodeAnalysis;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class AutoLayerRuleDefinition
{
    private readonly Perlin _perlin = new();
    public bool HasPerlin => PerlinActive;

    public AutoLayerRuleDefinition()
    {
        _perlin.Normalize = true;
        _perlin.AdjustScale(50, 1);
    }

    public bool Matches(LayerInstance li, LayerInstance source, int cx, int cy, int dirX = 1, int dirY = 1)
    {
        if (TileIds.Length == 0)
            return false;

        if (Chance <= 0 || (Chance < 1 && M.RandSeedCoords(li.Seed + Uid, cx, cy, 100) >= Chance * 100))
            return false;

        var seed = (int)(li.Seed * PerlinSeed);
        var x = (float)(cx * PerlinScale);
        var y = (float)(cy * PerlinScale);
        var perlinOctaves = (int)PerlinOctaves;
        if (HasPerlin && _perlin.PerlinNoise(seed, x, y, perlinOctaves) < 0)
            return false;

        var radius = Size / 2;
        for (var px = 0; px < Size; px++)
        {
            for (var py = 0; py < Size; py++)
            {
                var coordId = px + py * Size;
                if (Pattern[coordId] == 0) continue;

                var xx = cx + dirX * (px - radius);
                var yy = cy + dirY * (py - radius);
                var value = source.IsValid(xx, yy) ? source.GetIntGrid(xx, yy) : OutOfBoundsValue;

                if (value == null)
                    return false;

                if (Math.Abs(Pattern[coordId]) == Const.AutoLayerAnything)
                {
                    // "Anything" checks
                    if (Pattern[coordId] > 0 && value == 0)
                        return false;

                    if (Pattern[coordId] < 0 && value != 0)
                        return false;
                }
                else
                {
                    // Specific value checks
                    if (Pattern[coordId] > 0 && value != Pattern[coordId])
                        return false;

                    if (Pattern[coordId] < 0 && value == -Pattern[coordId])
                        return false;
                }
            }
        }

        return true;
    }
}