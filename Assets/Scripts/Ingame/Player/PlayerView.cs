using UnityEngine;

namespace Ingame
{

    [RequireComponent(typeof(PlayerModel))]
    public class PlayerView : AgentView
    {
        protected PlayerModel playerModel;

        protected void Awake()
        {
            base.Awake();
            playerModel = agentModel as PlayerModel;
        }
    }
}