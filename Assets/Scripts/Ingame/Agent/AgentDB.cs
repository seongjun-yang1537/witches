using System.Collections.Generic;
using UnityEngine;

public enum AgentType
{
    Player,
    Enemy,
}

public static class AgentDB
{
    private const string PATH_PREFIX = "Agents";

    private static Dictionary<AgentType, GameObject> cache = new();

    public static GameObject Get(AgentType agentType)
    {
        if (!cache.ContainsKey(agentType))
        {
            cache.Add(agentType, Resources.Load<GameObject>($"{PATH_PREFIX}/{agentType}"));
        }
        return cache[agentType];
    }
}