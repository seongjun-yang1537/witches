using UnityEngine;
using TMPro;

public class CityStatus : MonoBehaviour
{
    [Header("도시 정보")]
    [Tooltip("도시의 고유 이름")]
    public string cityName;

    [Header("점령 설정")]
    public ArmyStatus.TeamType owner;
    public float maxHP = 100f;
    public float regenPerSecond = 2f;
    public LayerMask capturerLayer;

    [Header("UI 세팅")]
    public Camera mainCamera;
    public TextMeshProUGUI uiText;
    public Vector3 uiOffset = Vector3.up;

    // 내부 상태
    private float currentHP;
    private RectTransform uiRect;
    private Canvas uiCanvas;
    private ArmyStatus currentCapturer = null;
    private float lastDisplayedHP = -1f; // 🔄 UI HP 업데이트 최적화용

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        uiRect = uiText.GetComponent<RectTransform>();
        uiCanvas = uiText.GetComponentInParent<Canvas>();

        currentHP = maxHP;
        UpdateCapturerLayer();  // 초기 소유자 기준 capturerLayer 설정
        UpdateLabel();          // 초기 UI 표시
    }

    void Update()
    {
        // UI 위치 갱신
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

        // 🔸 점령 중이면 체력 감소
        if (currentCapturer != null)
        {
            currentHP -= currentCapturer.captureSpeed * Time.deltaTime;
            if (currentHP <= 0f)
            {
                owner = currentCapturer.teamType;
                currentHP = maxHP * 0.3f;
                currentCapturer = null;

                UpdateCapturerLayer();

                string hexColor = (owner == ArmyStatus.TeamType.Red) ? "#FF0000" : "#0000FF";
                string coloredCity = $"<color={hexColor}>{cityName}</color>";
                string statusText = $"<b>Captured by {owner}</b>";
                WarlogManager.Instance?.LogEvent(coloredCity, statusText);

                UpdateLabel();
            }
        }
        else
        {
            // 🔹 점령 중이 아닐 때 체력 회복
            if (currentHP < maxHP)
            {
                currentHP += regenPerSecond * Time.deltaTime;
                if (currentHP > maxHP) currentHP = maxHP;
            }
        }

        // 🔄 체력이 변했을 경우에만 UI 갱신
        if (Mathf.FloorToInt(currentHP) != Mathf.FloorToInt(lastDisplayedHP))
        {
            lastDisplayedHP = currentHP;
            UpdateLabel();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & capturerLayer) == 0) return;

        var unit = other.GetComponent<ArmyStatus>();
        if (unit != null && unit.teamType != owner)
        {
            if (currentCapturer == null || currentCapturer == unit)
                currentCapturer = unit;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & capturerLayer) == 0) return;

        var unit = other.GetComponent<ArmyStatus>();
        if (unit != null && unit == currentCapturer)
        {
            currentCapturer = null;
        }
    }

    private void UpdateLabel()
    {
        uiText.text = $"{cityName}\n{owner}\nHP: {Mathf.FloorToInt(currentHP)}";
        uiText.color = (owner == ArmyStatus.TeamType.Blue) ? Color.blue : Color.red;
    }

    /// <summary>
    /// 현재 owner에 따라 capturerLayer를 자동 설정
    /// </summary>
    private void UpdateCapturerLayer()
    {
        string enemyLayerName = (owner == ArmyStatus.TeamType.Blue) ? "Red" : "Blue";
        capturerLayer = LayerMask.GetMask(enemyLayerName);
    }
}
