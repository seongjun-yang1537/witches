using UnityEngine;

namespace Ingame
{

    public class PlayerModel : AgentModel
    {

<<<<<<< Updated upstream
=======
        public PlayerState state;

        // public override void OnMovePhaseProgress(float progress)
        // {
        //     base.OnMovePhaseProgress(progress);
        //     SetState(PlayerState.Idle);
        // }

        public void SetState(PlayerState state)
        {
            this.state = state;
            onChangeState.Invoke(state);
        }
>>>>>>> Stashed changes
    }
}