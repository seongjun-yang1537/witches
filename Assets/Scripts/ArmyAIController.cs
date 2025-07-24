using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArmyStatus))]
public class ArmyAIController : MonoBehaviour
{
    public enum AIState
    {
        Advance,
        Retreat,
        Healing,
        Regroup
    }

    [Header("AI 설정")]
    public AIState currentState = AIState.Advance;
    [Range(0f, 1f)] public float retreatHPThreshold = 0.75f;

    [Header("적 감지 반경")]
    public float detectionRadius = 2f;

    private ArmyStatus armyStatus;
    private AIState previousState;
    private bool isInCityTrigger = false;

    void Awake()
    {
        armyStatus = GetComponent<ArmyStatus>();
        previousState = currentState;
    }

    void Update()
    {
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        // ── 1) Healing 재진입 체크 ──
        if (isInCityTrigger && armyStatus.currentHP < armyStatus.maxHP && currentState != AIState.Healing)
        {
            Debug.Log("[ArmyAI] Re-enter Healing (still in city)");
            armyStatus.StopMovement();
            currentState = AIState.Healing;
        }

        // ── 2) Healing 상태 처리 ──
        if (currentState == AIState.Healing)
        {
            HandleHealing();
            LogStateChange();
            return;
        }

        // ── 3) HP 기준 Advance/Retreat 결정 ──
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;
        AIState newState = (hpRatio <= retreatHPThreshold) ? AIState.Retreat : AIState.Advance;

        // 상태 전환 시 한 번만 Warlog 기록 (Retreat 진입)
        if (newState != currentState)
        {
            currentState = newState;
            if (currentState == AIState.Retreat)
            {
                // 팀 색상으로 유닛명 감싸기
                string hex = ColorUtility.ToHtmlStringRGB(armyStatus.TeamColor);
                string coloredName = $"<color=#{hex}>{armyStatus.title}</color>";
                WarlogManager.Instance.LogEvent(coloredName, "Start Retreat");
            }
        }

        LogStateChange();

        // ── 4) 상태별 행동 분기 ──
        switch (currentState)
        {
            case AIState.Advance:
                HandleAdvance();
                break;
            case AIState.Retreat:
                HandleRetreat();
                break;
            case AIState.Regroup:
                HandleRegroup();
                break;
        }
    }

    void LogStateChange()
    {
        if (currentState != previousState)
        {
            Debug.Log($"[ArmyAI] State changed: {previousState} → {currentState}");
            previousState = currentState;
        }
    }

    void HandleAdvance()
    {
        // 기존 Advance 로직 유지
        var hits = Physics.OverlapSphere(transform.position, detectionRadius, armyStatus.enemyLayer);
        if (hits.Length > 0)
        {
            ArmyStatus closest = null; float minD = float.MaxValue;
            foreach (var col in hits)
            {
                var e = col.GetComponent<ArmyStatus>();
                if (e == null) continue;
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d < minD) { minD = d; closest = e; }
            }
            if (closest != null)
            {
                armyStatus.ResumeMovement();
                armyStatus.MoveTo(closest.transform.position);
                return;
            }
        }

        CityStatus nearestCity = null; float minDist = float.MaxValue;
        foreach (var city in FindObjectsOfType<CityStatus>())
        {
            if (city.owner == armyStatus.teamType) continue;
            float d = Vector3.Distance(transform.position, city.transform.position);
            if (d < minDist) { minDist = d; nearestCity = city; }
        }
        if (nearestCity != null)
        {
            armyStatus.ResumeMovement();
            armyStatus.MoveTo(nearestCity.transform.position);
        }
        else currentState = AIState.Regroup;
    }

    void HandleRetreat()
    {
        // Retreat 상태에서는 이동만 수행 (로그는 Update에서 기록)
        var rp = RallyPointManager.Instance
            .GetNearestRetreatPoint(armyStatus.teamType, transform.position);
        if (rp != null)
        {
            armyStatus.ResumeMovement();
            armyStatus.MoveTo(rp.position);
        }
        else currentState = AIState.Regroup;
    }

    void HandleHealing()
    {
        // 전투 발생 시 Healing 중단
        var hits = Physics.OverlapSphere(transform.position, detectionRadius, armyStatus.enemyLayer);
        if (hits.Length > 0)
        {
            Debug.Log("[ArmyAI] Combat during HEALING → switch to ADVANCE");
            currentState = AIState.Advance;
            return;
        }

        // 지속 힐
        float before = armyStatus.currentHP;
        armyStatus.currentHP += armyStatus.healPerSecond * Time.deltaTime;
        Debug.Log($"[ArmyAI] Healing: {before:F1} → {armyStatus.currentHP:F1}");

        // 완전 회복 시 Advance로 전환 및 Warlog 기록
        if (armyStatus.currentHP >= armyStatus.maxHP)
        {
            armyStatus.currentHP = armyStatus.maxHP;
            Debug.Log("[ArmyAI] Fully healed → switch to ADVANCE");

            // 팀 색상으로 유닛명 감싸서 로그
            string hex = ColorUtility.ToHtmlStringRGB(armyStatus.TeamColor);
            string coloredName = $"<color=#{hex}>{armyStatus.title}</color>";
            WarlogManager.Instance.LogEvent(coloredName, "Healing Completed");

            isInCityTrigger = false;
            armyStatus.ResumeMovement();
            currentState = AIState.Advance;
            HandleAdvance();
        }
    }

    void HandleRegroup()
    {
        // TODO: Regroup 로직 구현
    }

    void OnTriggerEnter(Collider other)
    {
        var city = other.GetComponent<CityStatus>();
        if (city != null && city.owner == armyStatus.teamType)
        {
            Debug.Log("[ArmyAI] Entered friendly city → start HEALING");

            // 팀 색상으로 유닛명 감싸서 Healing 시작 로그
            string hex = ColorUtility.ToHtmlStringRGB(armyStatus.TeamColor);
            string coloredName = $"<color=#{hex}>{armyStatus.title}</color>";
            WarlogManager.Instance.LogEvent(coloredName, "Start Healing");

            isInCityTrigger = true;
            armyStatus.StopMovement();
            currentState = AIState.Healing;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var city = other.GetComponent<CityStatus>();
        if (city != null && city.owner == armyStatus.teamType)
        {
            Debug.Log("[ArmyAI] Exited friendly city trigger");
            isInCityTrigger = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
