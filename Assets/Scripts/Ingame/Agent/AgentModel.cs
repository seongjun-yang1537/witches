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

        public AgentModel aimtarget;
        public Vector3 aimDirection;

        [SerializeField]
        public Trajectory trajectory;

        protected void Start()
        {
            life = lifeMax;
        }
    }
}