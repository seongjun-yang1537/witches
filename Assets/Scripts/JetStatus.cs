using UnityEngine;
using TMPro;

public class JetStatus : MonoBehaviour
{
    public enum TeamType { Blue, Red }

    public enum JetType
    {
        Fighter,
        Attacker,
        ElectronicWarfare
    }

    public JetType jetType = JetType.Fighter;

    public CombatUnitType GetCombatUnitType()
    {
        switch (jetType)
        {
            case JetType.Fighter:
                return CombatUnitType.Fighter;
            case JetType.Attacker:
                return CombatUnitType.Attacker;
            case JetType.ElectronicWarfare:
                return CombatUnitType.ElectronicWarfare;
            default:
                return CombatUnitType.Fighter;
        }
    }

    [Header("기본 정보")]
    public TeamType teamType = TeamType.Blue;
    public string title = "Jet";
    public float maxHP = 100f;
    public float currentHP;

    [Header("UI 설정")]
    public GameObject uiPrefab;      // UnitTextUI 프리팹
    public Canvas uiCanvas;          // World Space UI용 캔버스
    private GameObject createdUI;    // 생성된 UI 오브젝트

    private TextMeshProUGUI tmpText;
    private WorldTextFollower follower;

    void Start()
    {
        currentHP = maxHP;

        SetUnitLabel(); // 병종 텍스트 자동 설정

        // ✅ UI 생성 및 설정
        if (uiPrefab != null && uiCanvas != null)
        {
            createdUI = Instantiate(uiPrefab, uiCanvas.transform);

            tmpText = createdUI.GetComponent<TextMeshProUGUI>();
            follower = createdUI.GetComponent<WorldTextFollower>();

            if (tmpText != null && follower != null)
            {
                follower.target = transform;
                follower.status = null;            // JetStatus에는 ArmyStatus가 아님
                follower.jetStatus = this;
                follower.uiText = tmpText;
                follower.label = title;

                // ✅ Blue는 뒤(-forward), Red는 앞(+forward)에 UI 표시
                float depth = transform.localScale.z;
                float padding = 0.5f;
                float distance = depth * 0.5f + padding;

                Vector3 direction = (teamType == TeamType.Red) ? transform.forward : -transform.forward;
                follower.offset = direction * distance;

                tmpText.alignment = (teamType == TeamType.Red)
                    ? TextAlignmentOptions.TopGeoAligned
                    : TextAlignmentOptions.BottomGeoAligned;
            }
            else
            {
                Debug.LogWarning("[JetStatus] WorldTextFollower 또는 TextMeshProUGUI가 프리팹에 없음");
            }
        }
        else
        {
            Debug.LogWarning("[JetStatus] uiPrefab 또는 uiCanvas가 연결되지 않음");
        }

        Debug.Log($"[JetStatus] 생성됨 - TeamType: {teamType}");
    }

    void OnDestroy()
    {
        if (createdUI != null)
            Destroy(createdUI);
    }

    // ✅ 현재 HP를 정수형으로 반환 (옵션)
    public int GetHPInt()
    {
        return Mathf.FloorToInt(currentHP);
    }

    // ✅ 데미지 처리 메서드 (옵션)
    public void ApplyDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0f)
        {
            Destroy(gameObject);
        }
    }
    void SetUnitLabel()
    {
        TextMeshPro label = GetComponentInChildren<TextMeshPro>();
        if (label == null) return;

        string code = jetType switch
        {
            JetType.Fighter => "Ftr",
            JetType.Attacker => "Atk",
            JetType.ElectronicWarfare => "EW",
            _ => "???"
        };

        label.text = code;

        label.color = teamType switch
        {
            TeamType.Blue => Color.blue,
            TeamType.Red => Color.red,
            _ => Color.white
        };
    }
}
