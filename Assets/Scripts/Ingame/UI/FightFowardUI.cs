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

        Vector3 handlePosition = -Vector3.one;

        void Awake()
        {
            camera = Camera.main;

            handleMeshRenderer = handle.GetComponent<MeshRenderer>();
            handleMeshRenderer.material = new Material(handleMeshRenderer.material);
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
            if (handlePosition == -Vector3.one)
            {
                handlePosition = tr.position + tr.forward * 3;
            }

            Vector3 startPosition = tr.position;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, handlePosition);

            handle.position = handlePosition;
        }

        private void SetHandleColor(Color color)
            => handleMeshRenderer.material.SetColor("_Color", color);

        private void UpdateHandle()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDraggingHandle = GetMouseInHandle();

                if (isDraggingHandle)
                    SetHandleColor(Color.yellow);
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDraggingHandle = false;
                SetHandleColor(Color.white);

                handlePosition = -Vector3.one;
            }

            if (isDraggingHandle)
                UpdateDraggingHandle();
        }

        private void UpdateDraggingHandle()
        {
            Vector3 screenPointOfHandle = camera.WorldToScreenPoint(handle.position);
            float zDepth = screenPointOfHandle.z;

            Vector2 mousePosition = Input.mousePosition;
            Vector3 projectionPosition = new Vector3(mousePosition.x, mousePosition.y, zDepth);
            Vector3 worldPosition = camera.ScreenToWorldPoint(projectionPosition);

            handlePosition = new Vector3(
                worldPosition.x,
                handle.position.y,
                worldPosition.z
            );
        }

        private bool GetMouseInHandle()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("UI"));
        }
    }
}