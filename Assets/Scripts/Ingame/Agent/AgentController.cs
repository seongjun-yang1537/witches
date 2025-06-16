using System;
using UnityEngine;

namespace Ingame
{
    [RequireComponent(typeof(AgentModel))]
    public class AgentController : SimulationBehaviour
    {
        private AgentModel _agentModel;
        public AgentModel AgentModel { get => _agentModel ??= GetComponent<AgentModel>(); }

        public override void OnMovePhaseStart()
        {
            Vector3 targetPosition = transform.position + AgentModel.planeSpec.minLength * transform.forward;
            AgentModel.SetTargetPosition(targetPosition);
        }

        public void MoveByTrajectory(float ratio)
        {
            if (AgentModel.trajectory == null) return;

            Vector3 nextPosition = AgentModel.trajectory.Interpolation(ratio);
            Vector3 direction = (nextPosition - transform.position).normalized;

            if (Mathf.Approximately(direction.sqrMagnitude, 0f)) return;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.position = nextPosition;
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