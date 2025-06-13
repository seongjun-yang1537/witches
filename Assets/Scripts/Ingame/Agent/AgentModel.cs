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

<<<<<<< Updated upstream
        protected void Awake()
        {

=======
        public AgentModel aimtarget;
        public Vector3 aimDirection;

        [SerializeField]
        public Trajectory trajectory;

        protected void Start()
        {
            life = lifeMax;
>>>>>>> Stashed changes
        }
    }
}