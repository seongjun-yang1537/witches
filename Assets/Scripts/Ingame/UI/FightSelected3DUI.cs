using Ingame;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class FightSelected3DUI : MonoBehaviour
    {
        public AgentControllerSystem system;
        public Transform selectedTransform;

        protected void Awake()
        {
            system.onSelect.AddListener(agent => OnSelect(agent));
            system.onDeSelect.AddListener(agent => OnDeSelect(agent));
        }

        protected void Start()
        {
            OnDeSelect(null);
        }

        private void OnSelect(AgentController agentController)
        {
            selectedTransform.gameObject.SetActive(true);
            selectedTransform.position = agentController.transform.position;
        }

        private void OnDeSelect(AgentController agentController)
        {
            selectedTransform.gameObject.SetActive(false);
        }
    }
}