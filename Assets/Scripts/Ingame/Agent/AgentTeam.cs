using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    public enum AgentTeam
    {
        ALLY,
        ENEMY0,
    }

    public static class AgentTeamDB
    {
        public static Dictionary<AgentTeam, Color> TEAM_COLOR = new()
        {
            { AgentTeam.ALLY, Color.white},
            { AgentTeam.ENEMY0, Color.red},
        };
    }
}