using UnityEngine;

namespace Ingame
{
    public class EnemyController : AgentController
    {
        private EnemyModel _enemyModel;
        protected EnemyModel EnemyModel { get => _enemyModel ??= GetComponent<EnemyModel>(); }
    }
}