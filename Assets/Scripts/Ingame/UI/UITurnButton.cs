using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace Ingame
{
    public class UITurnButton : SerializedMonoBehaviour
    {
        public GamePhase phase { get => GameManager.Instance.phase; }

        [SerializeField]
        public Dictionary<GamePhase, Image> phaseIcons;
        public Image imgRadial;

        protected void Awake()
        {
            GameManager.Instance.onPhase.AddListener(phase =>
            {
                imgRadial.fillAmount = 0f;
                SetVisibleAllIcons(false);
                phaseIcons[phase].gameObject.SetActive(true);
            });

            GameManager.Instance.onPhaseProgress.AddListener(progress =>
            {
                imgRadial.fillAmount = progress;
            });

            GameManager.Instance.onPhaseEnd.AddListener(() =>
            {
                imgRadial.fillAmount = 0f;
            });
        }

        protected void Start()
        {
            SetVisibleAllIcons(false);
            imgRadial.fillAmount = 0f;
        }

        private void SetVisibleAllIcons(bool active)
        {
            foreach (var img in phaseIcons.Values)
                img.gameObject.SetActive(active);
        }

        public void OnButtonNextPhase() => GameManager.Instance.NextPhase();
    }
}