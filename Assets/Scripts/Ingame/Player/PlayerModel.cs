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

        public void SetState(PlayerState state)
        {
            this.state = state;
            onChangeState.Invoke(state);
        }
    }
}