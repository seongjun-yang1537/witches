using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    [RequireComponent(typeof(Simulation))]
    public class AgentControllerSystem : SimulationBehaviour
    {
        public UnityEvent<AgentController> onSelect = new();
        public UnityEvent<AgentController> onDeSelect = new();

        public AgentController selectedAgent;

        private Camera mainCameara;
        protected void Awake()
        {
            base.Awake();
            mainCameara = Camera.main;
        }

        public override void OnMovePhase()
        {
            UpdateSelectedAgent();
        }

        public override void OnMovePhaseEnd()
        {
            onDeSelect.Invoke(null);
            selectedAgent = null;
        }

        public override void OnAttackPhase()
        {
            UpdateSelectedAgent();
        }

        public override void OnAttackPhaseEnd()
        {
            onDeSelect.Invoke(null);
            selectedAgent = null;
        }

        private void UpdateSelectedAgent()
        {
            if (Input.GetMouseButtonDown(0))
            {
                AgentController agent = GetAgentFromMousePosition(Input.mousePosition);
                if (agent != null) SelectAgent(agent);
            }
        }

        private void SelectAgent(AgentController agent)
        {
            if (selectedAgent != null)
            {
                onDeSelect.Invoke(selectedAgent);
                if (selectedAgent == agent)
                {
                    selectedAgent = null;
                    return;
                }
            }
            onSelect.Invoke(agent);
            selectedAgent = agent;
        }

        private AgentController GetAgentFromMousePosition(Vector2 mousePosition)
        {
            Ray ray = mainCameara.ScreenPointToRay(mousePosition);

            int layerMask = 1 << LayerMask.NameToLayer("Agent");
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

            foreach (var hit in hits)
            {
                AgentController model = hit.transform.GetComponent<AgentController>();
                if (model != null)
                {
                    return model;
                }
            }

            return null;
        }
    }
}