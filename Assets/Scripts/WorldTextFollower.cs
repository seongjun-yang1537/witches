using UnityEngine;
using TMPro;

public class WorldTextFollower : MonoBehaviour
{
    public Transform target;                          // 따라다닐 대상 오브젝트
    public Vector3 offset = Vector3.zero;             // 월드 기준 위치 오프셋 (방향 포함)
    public Camera mainCamera;                         // 사용할 카메라
    public TextMeshProUGUI uiText;                    // 연결된 텍스트
    public string label = "Unit";                     // 라벨 표시

    public ArmyStatus status;      // ✅ 기존 육군 유닛용
    public JetStatus jetStatus;    // ✅ 추가: 전투기 유닛용

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
            null,
            out anchoredPos);

        uiRectTransform.anchoredPosition = anchoredPos;

        // ✅ HP 및 유닛 타입 가져오기
        string hpLine = "HP: ?";
        CombatUnitType? selfType = null;

        if (status != null)
        {
            hpLine = $"HP: {status.GetHPInt():F1}";
            selfType = status.GetCombatUnitType();
        }
        else if (jetStatus != null)
        {
            hpLine = $"HP: {jetStatus.GetHPInt():F1}";
            selfType = jetStatus.GetCombatUnitType();
        }

        string armyLine = "";
        string jetLine = "";

        // ✅ selfType 이 유효할 경우에만 상성 정보 구성
        if (selfType.HasValue)
        {
            foreach (CombatUnitType otherType in System.Enum.GetValues(typeof(CombatUnitType)))
            {
                float multiplier = UnitAffinityManager.Instance.GetMultiplier(selfType.Value, otherType);
                string shortName = GetTypeAbbreviation(otherType);

                string colorWrapped;
                if (multiplier > 1.05f)
                    colorWrapped = $"<color=#FF5050>↑{shortName}</color>";
                else if (multiplier < 0.95f)
                    colorWrapped = $"<color=#5080FF>↓{shortName}</color>";
                else
                    colorWrapped = $"↔{shortName}";

                if (IsArmyType(otherType))
                    armyLine += colorWrapped + " ";
                else
                    jetLine += colorWrapped + " ";
            }
        }

        string affinityText = $"{armyLine.Trim()}\n{jetLine.Trim()}".Trim();

        // ✅ 팀 컬러에 따른 라벨 색상 지정
        string coloredLabel = label;
        Color teamColor = Color.white;

        if (status != null)
        {
            teamColor = status.teamType == ArmyStatus.TeamType.Red ? Color.red : Color.blue;
        }
        else if (jetStatus != null)
        {
            teamColor = jetStatus.teamType == JetStatus.TeamType.Red ? Color.red : Color.blue;
        }

        string hexColor = ColorUtility.ToHtmlStringRGB(teamColor);
        coloredLabel = $"<color=#{hexColor}>{label}</color>";

        // ✅ 최종 텍스트 출력
        if (string.IsNullOrWhiteSpace(affinityText))
            uiText.text = $"{coloredLabel}\n{hpLine}";
        else
            uiText.text = $"{coloredLabel}\n{hpLine}\n{affinityText}";
    }



    string GetTypeAbbreviation(CombatUnitType type)
    {
        switch (type)
        {
            case CombatUnitType.Infantry: return "Inf";
            case CombatUnitType.Armor: return "Arm";
            case CombatUnitType.AntiAir: return "AA";
            case CombatUnitType.Fighter: return "Ftr";
            case CombatUnitType.Attacker: return "Atk";
            case CombatUnitType.ElectronicWarfare: return "EW";
            default: return "???";
        }
    }

    bool IsArmyType(CombatUnitType type)
    {
        return type == CombatUnitType.Infantry
            || type == CombatUnitType.Armor
            || type == CombatUnitType.AntiAir;
    }

}