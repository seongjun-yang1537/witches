using System.Collections.Generic;
using UnityEngine;

public static class MaterialDB
{
    private const string PATH_PREFIX = "Materials";

    private static Dictionary<string, Material> cache = new();

    public static Material Get(string path)
    {
        if (!cache.ContainsKey(path))
        {
            cache.Add(path, Resources.Load<Material>($"{PATH_PREFIX}/{path}"));
        }
        return cache[path];
    }
}