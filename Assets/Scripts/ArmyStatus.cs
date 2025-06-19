// ✅ ArmyStatus.cs - Blue는 하단, Red는 상단에 UI 표시

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ArmyStatus : MonoBehaviour
{
    public enum ArmyType { Blue, Red }

    [Header("기본 정보")]
    public ArmyType armyType;
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
                Vector3 direction = (armyType == ArmyType.Red) ? transform.forward : -transform.forward;
                Vector3 offset = direction * distance;
                follower.offset = offset;

                tmpText.alignment = (armyType == ArmyType.Red)
                    ? TextAlignmentOptions.TopGeoAligned
                    : TextAlignmentOptions.BottomGeoAligned;
            }
        }
    }

    void Update()
    {
        if (attackers.Count == 0) return;

        foreach (var attacker in attackers)
        {
            if (attacker == null) continue;

            float multiplier = (armyType == ArmyType.Blue && attacker.armyType == ArmyType.Red) ? 1.5f :
                               (armyType == ArmyType.Red && attacker.armyType == ArmyType.Blue) ? 0.7f : 1f;

            float damage = attacker.baseDamagePerSecond * multiplier * Time.deltaTime;
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
