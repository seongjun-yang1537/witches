using UnityEngine;

namespace Ingame
{

    public class ProjectileModel : MonoBehaviour
    {
        public AgentModel owner;
        public AgentModel target;

        public Vector3 direction;
        public float speed;

        public float damage;

        public string type;

        protected void Update()
        {
            MoveTrackingTarget();
            if (owner == null || target == null) Destroy(gameObject);
        }

        private void MoveTrackingTarget()
        {
            if (target == null)
                return;

            Vector3 current = transform.position;
            Vector3 targetPos = target.transform.position;

            float distance = Vector3.Distance(current, targetPos);
            if (distance < 0.01f)
                return;

            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(current, targetPos, step);

            Vector3 dir = (targetPos - current).normalized;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        public void FromContext(ProjectileContext context)
        {
            this.owner = context.owner;
            this.target = context.target;
            this.direction = context.direction;
            this.speed = context.speed;
            this.damage = context.damage;
            this.type = context.type;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner == null || target == null) return;

            if (other.transform != target.transform)
                return;

            target.TakeDamage(owner, damage);
        }
    }
}