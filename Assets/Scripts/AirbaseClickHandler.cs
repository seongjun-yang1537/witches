using UnityEngine;

public class AirbaseClickHandler : MonoBehaviour
{
    public GameObject popupUI;
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        if (popupUI == null || mainCamera == null) return;

        popupUI.SetActive(true);

        // 1. Airbase의 중심을 화면 좌표로 변환
        Vector3 worldPos = transform.position;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // 2. 화면 좌표를 UI 로컬 좌표로 변환
        RectTransform canvasRect = popupUI.transform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,  // Overlay 모드일 경우
            out Vector2 localPos
        );

        // 3. 팝업 위치 설정 (왼쪽 아래 꼭짓점이 Airbase 위치에 오도록)
        RectTransform popupRect = popupUI.GetComponent<RectTransform>();
        popupRect.anchoredPosition = localPos;
    }
}
