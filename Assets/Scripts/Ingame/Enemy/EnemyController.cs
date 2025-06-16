using System;
using Corelib.Utils;
using UnityEngine;

namespace Ingame
{
    public class EnemyController : AgentController
    {
        private EnemyModel _enemyModel;
        protected EnemyModel EnemyModel { get => _enemyModel ??= GetComponent<EnemyModel>(); }

        private new MT19937 rng;

        protected void Awake()
        {
            base.Awake();

            int seed = (int)(DateTime.Now.Ticks & 0xFFFFFFFF);
            rng = MT19937.Create(gameObject.GetInstanceID() + seed);
        }

        public override void OnMovePhaseEnd()
        {
            base.OnMovePhaseEnd();

            Vector3 baseForward = transform.forward;
            float randomAngle = rng.NextFloat(-45f, 45f);
            Vector3 randomDir = Quaternion.Euler(0f, randomAngle, 0f) * baseForward;

            float randomLength = rng.NextFloat(EnemyModel.planeSpec.minLength, EnemyModel.planeSpec.maxLength);

            Vector3 targetPosition = transform.position + randomDir * randomLength;
            EnemyModel.SetTargetPosition(targetPosition);
            if (EnemyModel.trajectory.IsEmpty)
                EnemyModel.SetTargetPosition(targetPosition, "straight");
        }

        public override void OnAttackPhaseEnd()
        {
            base.OnAttackPhaseEnd();

            Vector3 baseForward = transform.forward;
            float randomAngle = rng.NextFloat(0f, 360f);
            Vector3 randomDir = Quaternion.Euler(0f, randomAngle, 0f) * baseForward;
            EnemyModel.SetAimDirection(randomDir);
        }
    }
}