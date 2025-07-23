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
        Regroup
    }

    [Header("AI 설정")]
    public AIState currentState = AIState.Advance;
    [Range(0f, 1f)]
    public float retreatHPThreshold = 0.75f;

    [Header("적 감지 설정")]
    [Tooltip("적 유닛을 감지할 반경")]
    public float detectionRadius = 2f;

    private ArmyStatus armyStatus;
    private AIState previousState;

    void Awake()
    {
        armyStatus = GetComponent<ArmyStatus>();
        previousState = currentState;
    }

    void Update()
    {
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        EvaluateState();

        if (currentState != previousState)
        {
            Debug.Log($"[ArmyAI] State changed: {previousState} → {currentState}");
            previousState = currentState;
        }

        switch (currentState)
        {
            case AIState.Advance: HandleAdvance(); break;
            case AIState.Retreat: HandleRetreat(); break;
            case AIState.Regroup: HandleRegroup(); break;
        }
    }

    private void EvaluateState()
    {
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;
        currentState = (hpRatio <= retreatHPThreshold)
            ? AIState.Retreat
            : AIState.Advance;
    }

    private void HandleAdvance()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            armyStatus.enemyLayer
        );
        if (hits.Length > 0)
        {
            ArmyStatus closest = null;
            float minD = float.MaxValue;
            foreach (var col in hits)
            {
                var enemy = col.GetComponent<ArmyStatus>();
                if (enemy == null) continue;
                float d = Vector3.Distance(transform.position, enemy.transform.position);
                if (d < minD)
                {
                    minD = d;
                    closest = enemy;
                }
            }
            if (closest != null)
            {
                armyStatus.MoveTo(closest.transform.position);
                return;
            }
        }

        CityStatus[] cities = FindObjectsOfType<CityStatus>();
        CityStatus nearestCity = null;
        float minDistCity = float.MaxValue;
        foreach (var city in cities)
        {
            if (city.owner == armyStatus.teamType) continue;
            float d = Vector3.Distance(transform.position, city.transform.position);
            if (d < minDistCity)
            {
                minDistCity = d;
                nearestCity = city;
            }
        }
        if (nearestCity != null)
            armyStatus.MoveTo(nearestCity.transform.position);
        else
        {
            Debug.LogWarning($"[ArmyAI] 적 도시가 없습니다: 팀={armyStatus.teamType}");
            currentState = AIState.Regroup;
        }
    }

    private void HandleRetreat()
    {
        Transform retreatPoint = RallyPointManager.Instance
            .GetNearestRetreatPoint(armyStatus.teamType, transform.position);
        if (retreatPoint != null)
            armyStatus.MoveTo(retreatPoint.position);
        else
        {
            Debug.LogWarning($"[ArmyAI] 뒤로 물러날 지점이 없습니다: 팀={armyStatus.teamType}");
            currentState = AIState.Regroup;
        }
    }

    private void HandleRegroup()
    {
        // TODO: 재정비 로직
    }

    public void SetState(AIState newState)
    {
        currentState = newState;
    }

    // 에디터 씬뷰에서 감지 반경을 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
