using System;

public static class SystemRandomExtensions
{
    public static int NextInclusive( this Random random, int min, int max ) => random.Next(min, max + 1);
    public static bool NextBool( this Random random ) => random.NextDouble() >= 0.5;
    /// <summary>
    /// Random single point precision value between 0-1
    /// </summary>
    /// <param name="random"></param>
    /// <returns></returns>
    public static float NextFloat(this Random random) => (float)random.NextDouble();
}
