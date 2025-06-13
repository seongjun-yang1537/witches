using UnityEngine;

namespace Ingame
{
    public class PlayerController : AgentController
    {
        private PlayerModel _playerModel;
        protected PlayerModel PlayerModel { get => _playerModel ??= GetComponent<PlayerModel>(); }
    }
}