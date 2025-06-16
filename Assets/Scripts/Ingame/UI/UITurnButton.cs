using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace Ingame
{
    public class UITurnButton : SimulationBehaviour
    {
        public GamePhase phase { get => GameManager.Instance.phase; }

        [SerializeField]
        public Dictionary<GamePhase, Image> phaseIcons;
        public Image imgRadial;

        protected void Start()
        {
            ResetUI();
        }

        public override void OnPhaseStart(GamePhase phase)
        {
            phaseIcons[phase].gameObject.SetActive(true);
        }

        public override void OnPhaseProgress(GamePhase phase, float progress, float deltaProgress)
        {
            imgRadial.fillAmount = progress;
        }

        public override void OnPhaseEnd(GamePhase phase)
        {
            ResetUI();
        }

        private void ResetUI()
        {
            SetVisibleAllIcons(false);
            imgRadial.fillAmount = 0f;
        }

        private void SetVisibleAllIcons(bool active)
        {
            foreach (var img in phaseIcons.Values)
                img.gameObject.SetActive(active);
        }

        public void OnButtonNextPhase() => gameManager.TriggerNextPhase();
    }
}