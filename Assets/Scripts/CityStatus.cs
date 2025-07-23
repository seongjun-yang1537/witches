using UnityEngine;
using TMPro;

public class CityStatus : MonoBehaviour
{
    [Header("���� ����")]
    [Tooltip("������ ���� �̸�")]
    public string cityName;

    [Header("���� ����")]
    [Tooltip("�ʱ� ������")]
    public ArmyStatus.TeamType owner;
    [Tooltip("���� �Ϸ���� �ɸ��� �ð� (��)")]
    public float captureTime = 3f;
    [Tooltip("���� ������ ���� ���̾�")]
    public LayerMask capturerLayer;

    [Header("UI ����")]
    [Tooltip("World��Screen ��ȯ�� ����� ī�޶�")]
    public Camera mainCamera;
    [Tooltip("���� �� ǥ�ÿ� TextMeshProUGUI")]
    public TextMeshProUGUI uiText;
    [Tooltip("UI�� �� ���� ���� ������")]
    public Vector3 uiOffset = Vector3.up;

    // ����
    private float captureProgress = 0f;
    private RectTransform uiRect;
    private Canvas uiCanvas;

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // uiText�� ���� RectTransform, Canvas ã�Ƶα�
        uiRect = uiText.GetComponent<RectTransform>();
        uiCanvas = uiText.GetComponentInParent<Canvas>();

        // �ʱ� �� ����
        UpdateLabel();
    }

    void Update()
    {
        // UI�� ���� ��ġ�� �°� �̵�
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
        // ������ ���̾ �ƴϸ� ����
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
        // �ؽ�Ʈ�� ���� ���� ����
        uiText.text = $"{cityName}\n{owner}";
        uiText.color = (owner == ArmyStatus.TeamType.Blue) ? Color.blue : Color.red;
    }
}
