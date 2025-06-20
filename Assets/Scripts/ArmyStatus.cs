using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ArmyStatus : MonoBehaviour
{
    public enum TeamType { Blue, Red }
    public enum UnitType { Armor, Infantry }

    [Header("기본 정보")]
    public TeamType teamType;
    public UnitType unitType;
    public string title = "Unit";
    public float maxHP = 100f;
    public float currentHP;

    public float baseDamagePerSecond = 10f;

    [Header("UI 설정")]
    public GameObject uiPrefab;
    public Canvas uiCanvas;
    private GameObject createdUI;

    private readonly List<ArmyStatus> attackers = new List<ArmyStatus>();

    void Start()
    {
        currentHP = maxHP;

        // ✅ UI 생성 및 WorldTextFollower 연결
        if (uiPrefab != null && uiCanvas != null)
        {
            createdUI = Instantiate(uiPrefab, uiCanvas.transform);
            var follower = createdUI.GetComponent<WorldTextFollower>();
            var tmpText = createdUI.GetComponent<TextMeshProUGUI>();

            if (follower != null && tmpText != null)
            {
                follower.target = this.transform;
                follower.status = this;
                follower.uiText = tmpText;
                follower.label = title;

                float depth = transform.localScale.z;
                float padding = 0.5f;
                float distance = depth * 0.5f + padding;

                // ✅ Blue는 뒤쪽(-forward), Red는 앞쪽(+forward) 방향에 표시
                Vector3 direction = (teamType == TeamType.Red) ? transform.forward : -transform.forward;
                Vector3 offset = direction * distance;
                follower.offset = offset;

                tmpText.alignment = (teamType == TeamType.Red)
                    ? TextAlignmentOptions.TopGeoAligned
                    : TextAlignmentOptions.BottomGeoAligned;
            }
        }

        Debug.Log($"[{title}] 생성됨 - TeamType: {teamType}");
    }

    void Update()
    {
        if (attackers.Count == 0) return;

        // ✅ 게임이 일시정지 상태일 경우 데미지 계산 중단
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        foreach (var attacker in attackers)
        {
            if (attacker == null) continue;

            // 🟦 유닛 타입 배율
            float typeMultiplier = 1f;
            if (attacker.unitType == UnitType.Armor && unitType == UnitType.Infantry)
                typeMultiplier = 1.3f;
            else if (attacker.unitType == UnitType.Infantry && unitType == UnitType.Armor)
                typeMultiplier = 0.8f;

            // 🟨 데미지 랜덤 ±5%
            float randomFactor = Random.Range(0.95f, 1.05f);

            float damage = attacker.baseDamagePerSecond * typeMultiplier * randomFactor * Time.deltaTime;
            currentHP -= damage;
        }

        if (currentHP <= 0f)
        {
            if (createdUI != null) Destroy(createdUI);
            Destroy(gameObject);
        }
    }

    public void AddAttacker(ArmyStatus attacker)
    {
        if (attacker != null && !attackers.Contains(attacker))
            attackers.Add(attacker);
    }

    public void RemoveAttacker(ArmyStatus attacker)
    {
        if (attacker != null)
            attackers.Remove(attacker);
    }

    public int GetHPInt()
    {
        return Mathf.FloorToInt(currentHP);
    }
}
