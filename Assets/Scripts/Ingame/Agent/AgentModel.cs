using UnityEngine;

namespace Ingame
{
    public class AgentModel : MonoBehaviour
    {
        public AgentTeam team;
        public AgentType agentType;
        public float life;
        public float lifeMax;

        public float speed;

        protected void Awake()
        {

        }
    }
}