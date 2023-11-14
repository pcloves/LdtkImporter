namespace LdtkImporter;

public static class M
{
    /// <summary>
    /// generate random number with x,y coordinate as seed, source code come from:
    /// <a href="https://github.com/deepnight/deepnightLibs/blob/7dd158925f02873d4bf751e1cdc953d98d77ad0b/src/dn/M.hx#L526">deepnightLibs</a>.
    /// </summary>
    /// <param name="seed">seed</param>
    /// <param name="x">x seed</param>
    /// <param name="y">y seed</param>
    /// <param name="max">max</param>
    /// <returns></returns>
    public static long RandSeedCoords(long seed, int x, int y, long max)
    {
        var h = seed + x * 374761393 + y * 668265263; // all constants are prime
        h = (h ^ (h >> 13)) * 1274126177;
        return (h ^ (h >> 16)) % max;
    }

    public static bool HasBit(int v, int bitIdx)
    {
        return (v & (1 << bitIdx)) != 0;
    }
}