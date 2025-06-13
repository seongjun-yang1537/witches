using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Corelib.SUI;
using Corelib.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Ingame
{
    public enum GamePhase
    {
        Pause,
        Move,
        Attack,
    }

    public class GameManager : Singleton<GameManager>
    {
        private readonly Dictionary<GamePhase, float> phaseDurations = new(){
            {GamePhase.Move, 3.0f},
            {GamePhase. Attack, 0.25f},
        };

        public float nowPhaseDuration { get => phaseDurations[phase]; }

        public UnityEvent<GamePhase> onPhase = new();
        public UnityEvent<float> onPhaseProgress = new();
        public UnityEvent onPhaseEnd = new();

        public List<AgentModel> agentModels { get => mapModel.agentModels; }
        [SerializeField]
        public Dictionary<AgentTeam, int> teamCount;

        public MapModel mapModel;

        public PlayerModel selectedPlayer;

        public GamePhase phase { get; private set; } = GamePhase.Attack;

        public int seed;
        public MT19937 rng;
        private Camera camera;

        public bool gameOver;

        private bool triggerNextTurn;

        private float phaseProgressTime;
        public float phaseProgress { get => 1 - phaseProgressTime / nowPhaseDuration; }

        protected void Awake()
        {
            if (seed == -1) rng = MT19937.Create();
            else rng = MT19937.Create(seed);

            camera = Camera.main;

            onPhaseProgress.AddListener(phase =>
            {
                selectedPlayer = null;
            });
            onPhase.AddListener(phase =>
            {
                if (phase == GamePhase.Attack) selectedPlayer = null;
            });
        }

        protected void Start()
        {
            SetPhase(GamePhase.Move);

            StartCoroutine(GamePhaseLoop());
        }

        protected void Update()
        {
            ProcessInputEvent();

            if (phaseProgressTime > 0f)
            {
                onPhaseProgress.Invoke(phaseProgress);
                phaseProgressTime -= Time.deltaTime;
            }
        }

        private void ProcessInputEvent()
        {
            if (phase == GamePhase.Move && phaseProgressTime <= 0f)
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
        }

        private IEnumerator GamePhaseLoop()
        {
            while (!gameOver)
            {
                yield return MovePhase();
                yield return AttackPhase();
            }
        }

        private IEnumerator MovePhase()
        {
            SetPhase(GamePhase.Move);
            phase = GamePhase.Move;

            yield return new WaitUntil(() => triggerNextTurn);

            triggerNextTurn = false;
            phaseProgressTime = phaseDurations[phase];

            yield return new WaitUntil(() => phaseProgressTime <= 0f);

            onPhaseEnd.Invoke();
        }

        private IEnumerator AttackPhase()
        {
            SetPhase(GamePhase.Attack);
            phase = GamePhase.Attack;

            yield return new WaitUntil(() => triggerNextTurn);

            triggerNextTurn = false;
            phaseProgressTime = phaseDurations[phase];

            yield return new WaitUntil(() => phaseProgressTime <= 0f);

            onPhaseEnd.Invoke();
        }

        public void NextPhase()
        {
            if (phaseProgressTime > 0f) return;
            triggerNextTurn = true;
        }

        public void SetPhase(GamePhase phase)
        {
            this.phase = phase;
            onPhase.Invoke(phase);
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

        public void DeadAgent(AgentModel agentModel)
        {
            agentModels.Remove(agentModel);
        }
    }
}