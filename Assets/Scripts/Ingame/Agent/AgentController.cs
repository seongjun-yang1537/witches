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
            AgentModel.SetTargetPosition(targetPosition, "straight");
            AgentModel.SetAimDirection(targetPosition);
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

        public virtual void Shoot(AgentModel target)
        {
            ProjectileContext context = new ProjectileContextBuilder()
                .SetOwner(AgentModel)
                .SetTarget(target)
                .SetDirection(AgentModel.aimDirection)
                .SetType("Default")
                .SetDamage(AgentModel.attackPower)
                .SetPosition(transform.position + 1.5f * AgentModel.aimDirection)
                .SetSpeed(10.0f)
                .Build();

            ProjectileSystem.Shoot(context);
        }

        public void TakeDamage(AgentController other, float damage)
        {
            AgentModel.life -= damage;
        }
    }
}