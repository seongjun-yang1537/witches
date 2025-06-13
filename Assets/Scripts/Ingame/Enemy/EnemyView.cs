using UnityEngine;

namespace Ingame
{

    [RequireComponent(typeof(EnemyModel))]
    public class EnemyView : AgentView
    {
        private EnemyModel _enemyModel;
        protected EnemyModel enemyModel { get => _enemyModel ??= GetComponent<EnemyModel>(); }
    }
}