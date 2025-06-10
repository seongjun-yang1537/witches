using UnityEngine;

namespace Ingame
{
    public class EnemyController : AgentController
    {
        protected EnemyModel enemyModel;
        protected void Awake()
        {
            enemyModel = agentModel as EnemyModel;
        }
    }
}