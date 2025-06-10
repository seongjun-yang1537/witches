using System.Collections.Generic;
using Corelib.SUI;
using Corelib.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ingame
{
    public class GameManager : Singleton<GameManager>
    {
        public List<AgentModel> agentModels;
        [SerializeField]
        public Dictionary<AgentTeam, int> teamCount;

        public MapModel mapModel;


        public Transform mapBody;
        public Vector3Int mapSize;

        public int seed;
        public MT19937 rng;

        protected void Awake()
        {
            if (seed == -1) rng = MT19937.Create();
            else rng = MT19937.Create(seed);
        }
    }
}