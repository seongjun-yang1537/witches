using UnityEngine;

namespace Ingame
{

    [RequireComponent(typeof(PlayerModel))]
    public class PlayerView : AgentView
    {
        private PlayerModel _playerModel;
        protected PlayerModel playerModel { get => _playerModel ??= GetComponent<PlayerModel>(); }
    }
}