using System.Collections;
using System.Collections.Generic;
using Corelib.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public enum GamePhase
    {
        Move,
        MoveProgress,
        Attack,
        COUNT,
    }

    public enum GameOverType
    {
        None,
        Win,
        Fail,
    }


    public class GameManager : Singleton<GameManager>
    {
        private readonly Dictionary<GamePhase, float> phaseDurations = new(){
            {GamePhase.Move, -1.0f},
            { GamePhase.MoveProgress, 3.0f},
            {GamePhase.Attack, 0.25f},
        };


        public UnityEvent<GamePhase> onPhaseStart = new();
        public UnityEvent<GamePhase, float, float> onPhaseProgress = new();
        public UnityEvent<GamePhase> onPhaseEnd = new();

        public Simulation simulation;

        [SerializeField]
        public GamePhase phase { get; private set; } = GamePhase.Move;
        public GameOverType gameover;

        public bool triggerNextPhase;
        public float phaseProgressTime;

        float prevProgress = 0f;

        protected void Awake()
        {
            onPhaseEnd.AddListener(phase => prevProgress = 0f);
        }

        protected void Start()
        {
            SetPhase(GamePhase.Move);
            StartCoroutine(GamePhaseLoop());
        }

        protected void Update()
        {
            UpdateGamePhase();
        }

        private void UpdateGamePhase()
        {
            switch (phase)
            {
                case GamePhase.Move:
                    simulation.OnMovePhase();
                    onPhaseProgress.Invoke(phase, 0f, 0f);
                    break;
                case GamePhase.MoveProgress:
                    float progress = 1 - (phaseProgressTime / phaseDurations[GamePhase.MoveProgress]);
                    float deltaProgress = progress - prevProgress;

                    simulation.OnMoveProgressPhase(progress, deltaProgress);
                    onPhaseProgress.Invoke(phase, progress, deltaProgress);

                    prevProgress = progress;
                    break;
                case GamePhase.Attack:
                    simulation.OnAttackPhase();
                    onPhaseProgress.Invoke(phase, 0f, 0f);
                    break;
            }
            phaseProgressTime -= Time.deltaTime;
        }

        public void TriggerNextPhase()
        {
            triggerNextPhase = true;
        }

        public void SetPhase(GamePhase phase)
        {
            this.phase = phase;
        }

        private IEnumerator GamePhaseLoop()
        {
            while (gameover == GameOverType.None)
            {
                yield return MovePhase();
                yield return MoveProgessPhase();
                yield return AttackPhase();
                gameover = GetGameOverState();
            }
        }

        private IEnumerator MovePhase()
        {
            SetPhase(GamePhase.Move);
            onPhaseStart.Invoke(GamePhase.Move);

            yield return new WaitUntil(() => triggerNextPhase);
            triggerNextPhase = false;

            onPhaseEnd.Invoke(GamePhase.Move);
        }

        private IEnumerator MoveProgessPhase()
        {
            SetPhase(GamePhase.MoveProgress);
            onPhaseStart.Invoke(GamePhase.MoveProgress);

            phaseProgressTime = phaseDurations[GamePhase.MoveProgress];
            yield return new WaitUntil(() => phaseProgressTime <= 0f);

            onPhaseEnd.Invoke(GamePhase.MoveProgress);
        }

        private IEnumerator AttackPhase()
        {
            SetPhase(GamePhase.Attack);
            onPhaseStart.Invoke(GamePhase.Attack);

            yield return new WaitUntil(() => triggerNextPhase);
            triggerNextPhase = false;

            onPhaseEnd.Invoke(GamePhase.Attack);
        }

        public GameOverType GetGameOverState()
        {
            return GameOverType.None;
        }
    }
}