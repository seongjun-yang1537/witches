using System;
using System.Collections.Generic;
using Corelib.SUI;
using Corelib.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ingame
{
    public class GameManager : Singleton<GameManager>
    {
        public List<AgentModel> agentModels;
        [SerializeField]
        public Dictionary<AgentTeam, int> teamCount;

        public MapModel mapModel;

        public PlayerModel selectedPlayer;

        public int seed;
        public MT19937 rng;
        private Camera camera;

        protected void Awake()
        {
            if (seed == -1) rng = MT19937.Create();
            else rng = MT19937.Create(seed);

            camera = Camera.main;
        }

        protected void Update()
        {
            ProcessInputEvent();
        }

        private void ProcessInputEvent()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlayerModel selected = GetPlayerModeFromMousePosition(Input.mousePosition);
                if (selected != null)
                {
                    SelectPlayer(selected);
                }
            }
        }

        public void SelectPlayer(PlayerModel playerModel)
        {
            if (selectedPlayer != null)
            {
                selectedPlayer.SetState(PlayerState.Idle);
                if (selectedPlayer == playerModel)
                {
                    selectedPlayer = null;
                    return;
                }
            }
            playerModel.SetState(PlayerState.Selected);
            selectedPlayer = playerModel;
        }

        private PlayerModel GetPlayerModeFromMousePosition(Vector2 mousePosition)
        {
            Ray ray = camera.ScreenPointToRay(mousePosition);

            int layerMask = 1 << LayerMask.NameToLayer("Agent");
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

            foreach (var hit in hits)
            {
                PlayerModel model = hit.transform.GetComponent<PlayerModel>();
                if (model != null)
                {
                    return model;
                }
            }

            return null;
        }
    }
}