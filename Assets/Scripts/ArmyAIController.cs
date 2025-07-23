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

    [Header("AI ����")]
    public AIState currentState = AIState.Advance;
    [Range(0f, 1f)]
    public float retreatHPThreshold = 0.75f;        // �ܿ� HP ����

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
            Debug.Log($"[ArmyAI] State changed: {previousState} �� {currentState}");
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

    // HP ������ ���� ��ȯ
    private void EvaluateState()
    {
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;
        currentState = (hpRatio <= retreatHPThreshold) ? AIState.Retreat : AIState.Advance;
    }

    // Advance �� �� ���� �̵�
    private void HandleAdvance()
    {
        armyStatus.HandleMovement();
    }

    // Retreat �� NavMeshAgent�� ���� ���� ����� ���� �������� �̵�
    private void HandleRetreat()
    {
        Transform retreatPoint = RallyPointManager.Instance
            .GetNearestRetreatPoint(armyStatus.teamType, transform.position);

        if (retreatPoint != null)
        {
            // NavMeshAgent ��� �̵� ȣ��
            armyStatus.MoveTo(retreatPoint.position);
        }
        else
        {
            Debug.LogWarning($"[ArmyAI] No retreat points found for team {armyStatus.teamType}");
            currentState = AIState.Regroup;
        }
    }

    // Regroup �� ���� ���ġ ����
    private void HandleRegroup()
    {
        // TODO: �Ʊ����� ��Ž�� �� ���� ����
    }

    // �ܺο��� ���� ���� ����
    public void SetState(AIState newState)
    {
        currentState = newState;
    }
}
