using System.Runtime.CompilerServices;
using UnityEngine;

namespace Ingame
{
    public class FightFowardUI : MonoBehaviour
    {
        public Transform handle;
        public LineRenderer lineRenderer;

        public PlayerModel selectedPlayer { get => GameManager.Instance.selectedPlayer; }

        private Camera camera;

        private MeshRenderer handleMeshRenderer;

        bool isDraggingHandle;

        protected void Awake()
        {
            camera = Camera.main;

            handleMeshRenderer = handle.GetComponent<MeshRenderer>();
            handleMeshRenderer.material = new Material(handleMeshRenderer.material);

            GameManager.Instance.onPhaseProgress.AddListener(phase =>
            {
                OnMouseUpHandle();
            });
            GameManager.Instance.onPhaseEnd.AddListener(() =>
            {
                OnMouseUpHandle();
            });
        }

        protected void Update()
        {
            UpdateActive();
            UpdateSelectedPlayer();
            UpdateHandle();
        }

        private void UpdateActive()
        {
            bool active = selectedPlayer != null;
            handle.gameObject.SetActive(active);
            lineRenderer.enabled = active;
        }

        private void UpdateSelectedPlayer()
        {
            if (selectedPlayer == null)
                return;

            Transform tr = selectedPlayer.transform;

            Vector3 startPosition = tr.position;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, selectedPlayer.targetPosition);

            handle.position = selectedPlayer.targetPosition;
        }

        private void SetHandleColor(Color color)
            => handleMeshRenderer.material.SetColor("_Color", color);

        private void UpdateHandle()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDraggingHandle = GetMouseInHandle();
                if (isDraggingHandle)
                    OnMouseDownHandle();
            }

            if (isDraggingHandle)
                OnMouseMoveHandle();

            if (Input.GetMouseButtonUp(0))
            {
                if (isDraggingHandle)
                    OnMouseUpHandle();
                isDraggingHandle = false;
            }
        }

        private bool GetMouseInHandle()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("UI"));
        }

        private void OnMouseDownHandle()
        {
            SetHandleColor(Color.yellow);
        }

        private void OnMouseMoveHandle()
        {
            Vector3 screenPointOfHandle = camera.WorldToScreenPoint(handle.position);
            float zDepth = screenPointOfHandle.z;

            Vector2 mousePosition = Input.mousePosition;
            Vector3 projectionPosition = new Vector3(mousePosition.x, mousePosition.y, zDepth);
            Vector3 worldPosition = camera.ScreenToWorldPoint(projectionPosition);

            selectedPlayer.targetPosition = new Vector3(
                worldPosition.x,
                handle.position.y,
                worldPosition.z
            );
        }

        private void OnMouseUpHandle()
        {
            SetHandleColor(Color.white);
        }
    }
}