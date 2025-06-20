using Ingame;
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

        // ✅ 팝업을 계층에서 가장 위로 올리기 (다른 UI 위에 뜨도록)
        popupUI.transform.SetAsLastSibling();

        // Airbase의 중심 → 스크린 좌표
        Vector3 worldPos = transform.position;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // 스크린 좌표 → UI 로컬 좌표
        RectTransform canvasRect = popupUI.transform.parent as RectTransform;
        if (canvasRect == null)
        {
            Debug.LogWarning("AirbaseClickHandler: PopupUI의 부모가 RectTransform이 아님");
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,  // Overlay 모드일 경우 null
            out Vector2 localPos))
        {
            Debug.LogWarning("AirbaseClickHandler: 좌표 변환 실패");
            return;
        }

        // 팝업 위치 지정 (왼쪽 아래 꼭짓점이 Airbase 위치에 오도록)
        RectTransform popupRect = popupUI.GetComponent<RectTransform>();
        if (popupRect != null)
            popupRect.anchoredPosition = localPos;
    }
}
