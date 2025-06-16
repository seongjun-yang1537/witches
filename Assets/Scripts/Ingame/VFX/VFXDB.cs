using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    public static class VFXDB
    {
        private const string PATH_PREFIX = "VFX";

        private static Dictionary<string, GameObject> cache = new();

        public static GameObject Get(string key)
        {
            if (!cache.ContainsKey(key))
            {
                cache.Add(key, Resources.Load<GameObject>($"{PATH_PREFIX}/{key}"));
            }
            return cache[key];
        }
    }
}