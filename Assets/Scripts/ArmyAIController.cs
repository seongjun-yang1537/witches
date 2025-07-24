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

        // ── 2) Healing 처리 ──
        if (currentState == AIState.Healing)
        {
            HandleHealing();
            LogStateChange();
            return;
        }

        // ── 3) HP 기준 Advance/Retreat ──
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;
        AIState newState = (hpRatio <= retreatHPThreshold) ? AIState.Retreat : AIState.Advance;

        // 상태 전환 시 한 번만 Warlog 기록
        if (newState != currentState)
        {
            currentState = newState;
            if (currentState == AIState.Retreat)
            {
                WarlogManager.Instance.LogEvent(armyStatus.title, "Start Retreat");
            }
        }

        // 상태 변경 로깅(디버그용)
        LogStateChange();

        // ── 4) 일반 상태 처리 ──
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
        // … (종전 코드 유지) :contentReference[oaicite:0]{index=0}
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
        // 단순 이동만 수행; 로그는 Update()에서 한 번만 기록
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
            WarlogManager.Instance.LogEvent(armyStatus.title, "Healing Completed");
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
            WarlogManager.Instance.LogEvent(armyStatus.title, "Start Healing");
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
