using Corelib.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ingame
{
    public class SimulationBehaviour : SerializedMonoBehaviour
    {
        protected GameManager gameManager { get => GameManager.Instance; }
        protected Simulation simulation { get => gameManager.simulation; }
        protected MT19937 rng { get => simulation.rng; }

        protected void Awake()
        {
            SubscribeEventHandler();
        }

        protected void SubscribeEventHandler()
        {
            gameManager.onPhaseStart.AddListener(phase =>
            {
                OnPhaseStart(phase);
                switch (phase)
                {
                    case GamePhase.Move:
                        OnMovePhaseStart();
                        break;
                    case GamePhase.MoveProgress:
                        OnMoveProgressPhaseStart();
                        break;
                    case GamePhase.Attack:
                        OnAttackPhaseStart();
                        break;
                }
            });

            gameManager.onPhaseProgress.AddListener((phase, progress) =>
            {
                OnPhaseProgress(phase, progress);
                switch (phase)
                {
                    case GamePhase.Move:
                        OnMovePhase();
                        break;
                    case GamePhase.MoveProgress:
                        OnMoveProgressPhase(progress);
                        break;
                    case GamePhase.Attack:
                        OnAttackPhase();
                        break;
                }
            });

            gameManager.onPhaseEnd.AddListener(phase =>
            {
                OnPhaseEnd(phase);
                switch (phase)
                {
                    case GamePhase.Move:
                        OnMovePhaseEnd();
                        break;
                    case GamePhase.MoveProgress:
                        OnMoveProgressPhaseEnd();
                        break;
                    case GamePhase.Attack:
                        OnAttackPhaseEnd();
                        break;
                }
            });
        }

        public virtual void OnPhaseStart(GamePhase phase) { }
        public virtual void OnPhaseProgress(GamePhase phase, float progress) { }
        public virtual void OnPhaseEnd(GamePhase phase) { }

        public virtual void OnMovePhaseStart() { }

        public virtual void OnMovePhase() { }

        public virtual void OnMovePhaseEnd() { }

        public virtual void OnMoveProgressPhaseStart() { }

        public virtual void OnMoveProgressPhase(float progress) { }
        public virtual void OnMoveProgressPhaseEnd() { }


        public virtual void OnAttackPhaseStart() { }
        public virtual void OnAttackPhase() { }
        public virtual void OnAttackPhaseEnd() { }
    }
}