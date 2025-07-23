using UnityEngine;
using TMPro;

public class CityStatus : MonoBehaviour
{
    [Header("도시 정보")]
    [Tooltip("도시의 고유 이름")]
    public string cityName;

    [Header("점령 설정")]
    [Tooltip("초기 소유팀")]
    public ArmyStatus.TeamType owner;
    [Tooltip("점령 완료까지 걸리는 시간 (초)")]
    public float captureTime = 3f;
    [Tooltip("점령 가능한 유닛 레이어")]
    public LayerMask capturerLayer;

    [Header("UI 세팅")]
    [Tooltip("World→Screen 변환에 사용할 카메라")]
    public Camera mainCamera;
    [Tooltip("도시 라벨 표시용 TextMeshProUGUI")]
    public TextMeshProUGUI uiText;
    [Tooltip("UI가 떠 있을 월드 오프셋")]
    public Vector3 uiOffset = Vector3.up;

    // 내부
    private float captureProgress = 0f;
    private RectTransform uiRect;
    private Canvas uiCanvas;

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // uiText가 속한 RectTransform, Canvas 찾아두기
        uiRect = uiText.GetComponent<RectTransform>();
        uiCanvas = uiText.GetComponentInParent<Canvas>();

        // 초기 라벨 갱신
        UpdateLabel();
    }

    void Update()
    {
        // UI를 월드 위치에 맞게 이동
        Vector3 worldPos = transform.position + uiOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f)
        {
            uiText.enabled = false;
        }
        else
        {
            uiText.enabled = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiCanvas.transform as RectTransform,
                screenPos,
                uiCanvas.worldCamera,
                out Vector2 anchoredPos);
            uiRect.anchoredPosition = anchoredPos;
        }
    }

    void OnTriggerStay(Collider other)
    {
        // 지정된 레이어가 아니면 리턴
        if (((1 << other.gameObject.layer) & capturerLayer) == 0) return;

        var unit = other.GetComponent<ArmyStatus>();
        if (unit != null && unit.teamType != owner)
        {
            captureProgress += Time.deltaTime;
            if (captureProgress >= captureTime)
            {
                owner = unit.teamType;
                captureProgress = 0f;
                UpdateLabel();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & capturerLayer) == 0) return;

        var unit = other.GetComponent<ArmyStatus>();
        if (unit != null && unit.teamType != owner)
        {
            captureProgress = 0f;
        }
    }

    private void UpdateLabel()
    {
        // 텍스트와 색상 동시 설정
        uiText.text = $"{cityName}\n{owner}";
        uiText.color = (owner == ArmyStatus.TeamType.Blue) ? Color.blue : Color.red;
    }
}
