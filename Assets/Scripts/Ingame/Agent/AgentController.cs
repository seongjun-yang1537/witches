using UnityEngine;

namespace Ingame
{
    [RequireComponent(typeof(AgentModel))]
    public class AgentController : MonoBehaviour
    {
        private AgentModel _agentModel;
        public AgentModel AgentModel { get => _agentModel ??= GetComponent<AgentModel>(); }

        public virtual void Select()
        {

        }

        public virtual void DeSelect()
        {

        }

        public void Shoot(AgentModel target)
        {
            // ProjectileContext context = new ProjectileContextBuilder()
            //     .SetOwner(agentModel)
            //     .SetTarget(target)
            //     .SetDirection(agentModel.aimDirection)
            //     .SetType("Default")
            //     .SetDamage(agentModel.attackPower)
            //     .SetPosition(transform.position)
            //     .SetSpeed(3.0f)
            //     .Build();

            // ProjectileManager.Instance.Shoot(context);
        }
    }
}