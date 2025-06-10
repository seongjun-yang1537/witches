using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("시네머신 가상 카메라")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("카메라 이동(패닝) 설정")]
    [SerializeField] private float panSpeed = 20f;
    private Vector3 lastMousePosition;

    [Header("카메라 줌 (높이) 설정")]
    [SerializeField] private float zoomSpeed = 30f;
    [SerializeField] private float minHeight = 10f;
    [SerializeField] private float maxHeight = 80f;

    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            transform.Translate(-delta.x * panSpeed * Time.deltaTime, -delta.y * panSpeed * Time.deltaTime, 0f);
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 newPosition = transform.position;
            newPosition.y -= scroll * zoomSpeed;
            newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);
            transform.position = newPosition;
        }
    }
}