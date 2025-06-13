using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    public class FightTrajectory3DUI : SimulationBehaviour
    {
        public AgentControllerSystem system;

        public Transform handle;
        public LineRenderer lineRenderer;

        private AgentController selectedeAgent { get => system.selectedAgent; }
        private AgentModel agentModel { get => selectedeAgent.AgentModel; }

        protected void Awake()
        {
            base.Awake();
            system.onSelect.AddListener(agent => OnSelect(agent));
            system.onDeSelect.AddListener(agent => OnDeSelect(agent));
        }

        protected void Start()
        {
            OnDeSelect(null);
        }

        public override void OnMovePhase()
        {
            if (selectedeAgent == null) return;
            handle.transform.position = agentModel.targetPosition;

            Trajectory trajectory = agentModel.trajectory;
            lineRenderer.positionCount = trajectory.Count;
            lineRenderer.SetPositions(trajectory.samples.ToArray());
        }

        private void OnSelect(AgentController agentController)
        {

        }

        private void OnDeSelect(AgentController agentController)
        {

        }
    }
}