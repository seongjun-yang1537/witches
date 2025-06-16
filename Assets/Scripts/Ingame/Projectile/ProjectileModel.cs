using UnityEngine;

namespace Ingame
{

    public class ProjectileModel : SimulationBehaviour
    {
        public AgentModel owner;
        public AgentModel target;

        public Vector3 direction;
        public float speed;

        public float damage;

        public string type;

        bool isMoveProgressPhase;

        public void FromContext(ProjectileContext context)
        {
            this.owner = context.owner;
            this.target = context.target;
            this.direction = context.direction;
            this.speed = context.speed;
            this.damage = context.damage;
            this.type = context.type;
        }

        protected void Update()
        {
            float sqrOwnerDistance = (owner.transform.position - transform.position).sqrMagnitude;
            if (sqrOwnerDistance > 1000.0f)
                Explosion();
        }

        public override void OnMoveProgressPhaseStart()
        {
            isMoveProgressPhase = true;
        }

        public override void OnMoveProgressPhase(float progress, float deltaProgress)
        {
            transform.position += direction * deltaProgress * speed;
        }

        public override void OnMoveProgressPhaseEnd()
        {
            isMoveProgressPhase = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            AgentController otherAgent = other.gameObject.GetComponent<AgentController>();
            if (otherAgent == null || otherAgent.AgentModel.team == owner.team) return;

            otherAgent.TakeDamage(otherAgent, damage);
            Explosion();
        }

        private void Explosion()
        {
            if (!isMoveProgressPhase) return;
            Destroy(gameObject);
        }

        protected void OnDestroy()
        {
            VFXModel model = VFXSystem.Spawn("explosion", 1.0f);
            model.transform.position = transform.position;
        }
    }
}