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
        // 전투 중단 후에도 도시 트리거 내라면 다시 Healing 상태로
        if (isInCityTrigger && armyStatus.currentHP < armyStatus.maxHP && currentState != AIState.Healing)
        {
            Debug.Log("[ArmyAI] Re-enter Healing (still in city)");
            armyStatus.StopMovement();
            currentState = AIState.Healing;
        }

        // ── 2) Healing 처리 ──
        if (currentState == AIState.Healing)
        {
            HandleHealing();
            LogStateChange();
            return;
        }

        // ── 3) HP 기준 Advance/Retreat ──
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;
        currentState = (hpRatio <= retreatHPThreshold)
            ? AIState.Retreat
            : AIState.Advance;

        LogStateChange();

        // ── 4) 일반 상태 처리 ──
        switch (currentState)
        {
            case AIState.Advance: HandleAdvance(); break;
            case AIState.Retreat: HandleRetreat(); break;
            case AIState.Regroup: HandleRegroup(); break;
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
        // 적 유닛 감지 및 교전
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

        // 적 도시로 전진
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
        // 전투 발생 시 중단
        var hits = Physics.OverlapSphere(transform.position, detectionRadius, armyStatus.enemyLayer);
        if (hits.Length > 0)
        {
            Debug.Log("[ArmyAI] Combat during HEALING → switch to ADVANCE");
            currentState = AIState.Advance;
            return;
        }

        // 회복
        float before = armyStatus.currentHP;
        armyStatus.currentHP += armyStatus.healPerSecond * Time.deltaTime;
        Debug.Log($"[ArmyAI] Healing: {before:F1} → {armyStatus.currentHP:F1}");

        // 완전 회복 시
        if (armyStatus.currentHP >= armyStatus.maxHP)
        {
            armyStatus.currentHP = armyStatus.maxHP;
            Debug.Log("[ArmyAI] Fully healed → switch to ADVANCE");
            isInCityTrigger = false;

            armyStatus.ResumeMovement();
            currentState = AIState.Advance;
            HandleAdvance();
        }
    }

    void HandleRegroup()
    {
        // TODO: 재정비 로직
    }

    void OnTriggerEnter(Collider other)
    {
        var city = other.GetComponent<CityStatus>();
        if (city != null && city.owner == armyStatus.teamType)
        {
            Debug.Log($"[ArmyAI] Entered friendly city → start HEALING");
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
