using UnityEngine;
using TMPro;

public class WorldTextFollower : MonoBehaviour
{
    public Transform target;                          // 따라다닐 대상 오브젝트
    public Vector3 offset = Vector3.zero;             // 월드 기준 위치 오프셋 (방향 포함)
    public Camera mainCamera;                         // 사용할 카메라
    public TextMeshProUGUI uiText;                    // 연결된 텍스트
    public string label = "Unit";                     // 라벨 표시
    public ArmyStatus status;                         // HP 정보 등

    private RectTransform uiRectTransform;
    private Canvas canvas;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        uiRectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (uiText == null)
            uiText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        if (uiText == null || uiRectTransform == null || canvas == null) return;

        // ✅ offset은 이미 forward 또는 -forward 방향을 포함한 월드 좌표 보정값
        Vector3 worldPosition = target.position + offset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z < 0)
        {
            uiText.enabled = false;
            return;
        }

        uiText.enabled = true;

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            null,  // Overlay 모드일 경우 카메라 = null
            out anchoredPos);

        uiRectTransform.anchoredPosition = anchoredPos;

        // ✅ 텍스트 구성
        string hpLine = (status != null) ? $"HP: {status.GetHPInt()}" : "HP: ?";
        string buffLine = "Infantry: -";

        uiText.text = $"{label}\n{hpLine}\n{buffLine}";
    }
}