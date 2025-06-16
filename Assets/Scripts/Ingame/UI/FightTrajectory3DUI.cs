using System.Collections.Generic;
using System.Linq;
using Corelib.Utils;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Rendering;
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

        private Camera mainCamera;

        bool isDraggingHandle;

        protected void Awake()
        {
            base.Awake();
            mainCamera = Camera.main;

            system.onSelect.AddListener(agent => OnSelect(agent));
            system.onDeSelect.AddListener(agent => OnDeSelect(agent));
        }

        protected void Start()
        {
            OnDeSelect(null);
        }

        public void SetActiveUI(bool active)
        {
            handle.gameObject.SetActive(active);
            lineRenderer.gameObject.SetActive(active);
        }

        public override void OnMovePhaseStart()
        {
            SetActiveUI(true);
        }

        public override void OnMovePhase()
        {
            SetActiveUI(selectedeAgent != null);

            if (selectedeAgent == null) return;
            UpdateHandle();

            handle.transform.position = agentModel.targetPosition;

            Trajectory trajectory = agentModel.trajectory;
            lineRenderer.positionCount = trajectory.Count;
            lineRenderer.SetPositions(trajectory.samples.ToArray());
        }

        public override void OnMovePhaseEnd()
        {
            SetActiveUI(false);
        }

        private void OnSelect(AgentController agentController)
        {

        }

        private void OnDeSelect(AgentController agentController)
        {

        }

        private void UpdateHandle()
        {
            if (Input.GetMouseButtonDown(0) && IsClickHandle())
                isDraggingHandle = true;

            if (isDraggingHandle)
                UpdateTargetPosition();

            if (Input.GetMouseButtonUp(0))
                isDraggingHandle = false;
        }

        private void UpdateTargetPosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, handle.position.y, 0));

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldPosition = ray.GetPoint(enter);
                if (agentModel.IsCanTargetPosition(worldPosition))
                    agentModel.SetTargetPosition(worldPosition);
            }
        }

        private bool IsClickHandle()
        {
            Vector2 mousePosition = Input.mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, 1 << LayerMask.NameToLayer("UI"));
            return hits.Any(hit => hit.transform == handle);
        }
    }
}