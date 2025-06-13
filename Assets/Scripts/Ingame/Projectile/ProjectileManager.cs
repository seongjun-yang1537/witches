using Corelib.Utils;
using UnityEngine;

namespace Ingame
{
    public class ProjectileContextBuilder
    {
        private readonly ProjectileContext _context = new();

        public ProjectileContextBuilder SetOwner(AgentModel owner)
        {
            _context.owner = owner;
            return this;
        }

        public ProjectileContextBuilder SetTarget(AgentModel target)
        {
            _context.target = target;
            return this;
        }

        public ProjectileContextBuilder SetPosition(Vector3 position)
        {
            _context.position = position;
            return this;
        }

        public ProjectileContextBuilder SetDirection(Vector3 direction)
        {
            _context.direction = direction;
            return this;
        }

        public ProjectileContextBuilder SetSpeed(float speed)
        {
            _context.speed = speed;
            return this;
        }

        public ProjectileContextBuilder SetDamage(float damage)
        {
            _context.damage = damage;
            return this;
        }

        public ProjectileContextBuilder SetType(string type)
        {
            _context.type = type;
            return this;
        }

        public ProjectileContext Build()
        {
            return _context;
        }
    }

    public class ProjectileContext
    {
        public AgentModel owner;
        public AgentModel target;

        public Vector3 position;
        public Vector3 direction;
        public float speed;

        public float damage;

        public string type;
    }

    public class ProjectileManager : Singleton<ProjectileManager>
    {
        public ProjectileModel Shoot(ProjectileContext context)
        {
            GameObject go = Instantiate(ProjectileDB.Get(context.type));

            Transform tr = go.transform;
            tr.SetParent(transform);
            tr.position = context.position;

            ProjectileModel model = go.GetComponent<ProjectileModel>();
            model.FromContext(context);

            return model;
        }
    }
}