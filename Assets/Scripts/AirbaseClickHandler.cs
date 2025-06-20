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

        // 1. Airbase�� �߽��� ȭ�� ��ǥ�� ��ȯ
        Vector3 worldPos = transform.position;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // 2. ȭ�� ��ǥ�� UI ���� ��ǥ�� ��ȯ
        RectTransform canvasRect = popupUI.transform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,  // Overlay ����� ���
            out Vector2 localPos
        );

        // 3. �˾� ��ġ ���� (���� �Ʒ� �������� Airbase ��ġ�� ������)
        RectTransform popupRect = popupUI.GetComponent<RectTransform>();
        popupRect.anchoredPosition = localPos;
    }
}
