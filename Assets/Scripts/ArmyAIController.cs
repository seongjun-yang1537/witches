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
    public float retreatHPThreshold = 0.75f;        // 잔여 HP 비율

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

    // HP 비율로 상태 전환
    private void EvaluateState()
    {
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;
        currentState = (hpRatio <= retreatHPThreshold) ? AIState.Retreat : AIState.Advance;
    }

    // Advance → 적 추적 이동
    private void HandleAdvance()
    {
        armyStatus.HandleMovement();
    }

    // Retreat → NavMeshAgent를 통해 가장 가까운 후퇴 지점으로 이동
    private void HandleRetreat()
    {
        Transform retreatPoint = RallyPointManager.Instance
            .GetNearestRetreatPoint(armyStatus.teamType, transform.position);

        if (retreatPoint != null)
        {
            // NavMeshAgent 기반 이동 호출
            armyStatus.MoveTo(retreatPoint.position);
        }
        else
        {
            Debug.LogWarning($"[ArmyAI] No retreat points found for team {armyStatus.teamType}");
            currentState = AIState.Regroup;
        }
    }

    // Regroup → 향후 재배치 로직
    private void HandleRegroup()
    {
        // TODO: 아군·적 재탐색 후 상태 결정
    }

    // 외부에서 강제 상태 설정
    public void SetState(AIState newState)
    {
        currentState = newState;
    }
}
