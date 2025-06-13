using UnityEngine;

namespace Ingame
{
    [RequireComponent(typeof(Simulation))]
    public class AgentControllerSystem : SimulationBehaviour
    {
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
            selectedAgent = null;
        }

        public override void OnAttackPhase()
        {
            UpdateSelectedAgent();
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
                selectedAgent.DeSelect();
                if (selectedAgent == agent)
                {
                    selectedAgent = null;
                    return;
                }
            }
            agent.Select();
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