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

        // public void FromContext(ProjectileContext context)
        // {
        //     this.owner = context.owner;
        //     this.target = context.target;
        //     this.direction = context.direction;
        //     this.speed = context.speed;
        //     this.damage = context.damage;
        //     this.type = context.type;
        // }
    }
}