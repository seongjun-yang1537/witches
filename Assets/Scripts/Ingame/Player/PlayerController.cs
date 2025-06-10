using UnityEngine;

namespace Ingame
{
    public class PlayerController : AgentController
    {
        protected PlayerModel playerModel;
        protected void Awake()
        {
            playerModel = agentModel as PlayerModel;
        }
    }
}