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

            playerModel.onChangeState.AddListener(state =>
            {
                switch (state)
                {
                    case PlayerState.Idle:
                        {
                            UpdateMaterialColor(GetMyTeamColor());
                        }
                        break;
                    case PlayerState.Selected:
                        {
                            UpdateMaterialColor(Color.yellow);
                        }
                        break;
                }
            });
        }
    }
}