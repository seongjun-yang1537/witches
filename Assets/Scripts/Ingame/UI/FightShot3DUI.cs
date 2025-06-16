using System.Collections.Generic;
using System.Linq;
using Corelib.Utils;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

namespace Ingame
{
    public class FightShot3DUI : SimulationBehaviour
    {
        public AgentControllerSystem system;

        public Transform handle;

        private AgentController selectedeAgent { get => system.selectedAgent; }
        private AgentModel agentModel { get => selectedeAgent.AgentModel; }

        public float radius { get => agentModel.radius; }

        private Camera mainCamera;

        bool isDraggingHandle;

        protected void Awake()
        {
            base.Awake();
            mainCamera = Camera.main;
        }

        protected void Start()
        {
            SetActiveUI(false);
        }

        public void SetActiveUI(bool active)
        {
            handle.gameObject.SetActive(active);
        }

        public override void OnAttackPhaseStart()
        {
            SetActiveUI(true);
        }

        public override void OnAttackPhase()
        {
            SetActiveUI(selectedeAgent != null);

            if (selectedeAgent == null) return;
            UpdateHandle();

            Vector3 aimDirection = agentModel.aimDirection;
            handle.transform.position = agentModel.transform.position + 1.5f * aimDirection;
            handle.transform.eulerAngles = new Vector3(
                90f,
                Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg,
                0f
            );
        }

        public override void OnAttackPhaseEnd()
        {
            SetActiveUI(false);
        }

        private void UpdateHandle()
        {
            if (Input.GetMouseButtonDown(0))
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
                float sqrDistance = (worldPosition - agentModel.transform.position).sqrMagnitude;
                if (sqrDistance > 3.0f) return;

                Vector3 direciotn = (worldPosition - agentModel.transform.position).normalized;
                agentModel.SetAimDirection(direciotn);
            }
        }
    }
}