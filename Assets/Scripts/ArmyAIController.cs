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
    public float retreatHPThreshold = 0.75f;        // 소모 비율이 아닌 잔여 비율임

    private ArmyStatus armyStatus;
    private AIState previousState;

    void Awake()
    {
        armyStatus = GetComponent<ArmyStatus>();
        // 초기 이전 상태 세팅
        previousState = currentState;
    }

    void Update()
    {
        // 게임 일시정지 시 AI 동작 멈춤
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        // 상태 평가 및 전환
        EvaluateState();

        // 상태 변경 감지 및 로그 출력
        if (currentState != previousState)
        {
            Debug.Log($"[ArmyAI] State changed: {previousState} → {currentState}");
            previousState = currentState;
        }

        // 상태별 핸들러 호출
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

    // 상태 전환 조건 판단
    private void EvaluateState()
    {
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;        
        currentState = (hpRatio <= retreatHPThreshold) ? AIState.Retreat : AIState.Advance;
    }

    // Advance 상태 → 가장 가까운 적에게 이동
    private void HandleAdvance()
    {
        armyStatus.HandleMovement();
    }

    // Retreat 상태 → 안전 지점으로 후퇴 또는 대기
    private void HandleRetreat()
    {
        // 매니저에서 가장 가까운 후퇴 지점 찾기
        Transform targetPoint = RallyPointManager.Instance
            .GetNearestRetreatPoint(armyStatus.teamType, transform.position);

        if (targetPoint != null)
        {
            Vector3 dir = (targetPoint.position - transform.position).normalized;
            transform.position += dir * armyStatus.speed * Time.deltaTime;
        }
        else
        {
            // 포인트가 없는 경우 대기 상태로 전환
            Debug.LogWarning("[ArmyAI] No retreat points found for team " + armyStatus.teamType);
            currentState = AIState.Regroup;
        }
    }


    // Regroup 상태 → 주변 정보 재수집 후 상태 전환
    private void HandleRegroup()
    {
        // TODO: 주변 아군·적 정보 재수집 및 다음 행동 결정
    }

    // 외부에서 상태 강제 변경 가능
    public void SetState(AIState newState)
    {
        currentState = newState;
    }
}
