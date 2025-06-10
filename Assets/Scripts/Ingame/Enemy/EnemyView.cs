using UnityEngine;

namespace Ingame
{

    [RequireComponent(typeof(EnemyModel))]
    public class EnemyView : AgentView
    {
        protected EnemyModel enemyModel;

        protected void Awake()
        {
            base.Awake();
            enemyModel = agentModel as EnemyModel;
        }
    }
}