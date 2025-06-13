using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public enum PlayerState
    {
        Idle,
        Selected,
    }


    public class PlayerModel : AgentModel
    {
        public UnityEvent<PlayerState> onChangeState = new();

        public PlayerState state;

        public override void OnMovePhaseProgress(float progress)
        {
            base.OnMovePhaseProgress(progress);
            SetState(PlayerState.Idle);
        }

        public void SetState(PlayerState state)
        {
            this.state = state;
            onChangeState.Invoke(state);
        }
    }
}