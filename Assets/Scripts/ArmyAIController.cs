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
    public float retreatHPThreshold = 0.75f;        // �Ҹ� ������ �ƴ� �ܿ� ������

    private ArmyStatus armyStatus;
    private AIState previousState;

    void Awake()
    {
        armyStatus = GetComponent<ArmyStatus>();
        // �ʱ� ���� ���� ����
        previousState = currentState;
    }

    void Update()
    {
        // ���� �Ͻ����� �� AI ���� ����
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        // ���� �� �� ��ȯ
        EvaluateState();

        // ���� ���� ���� �� �α� ���
        if (currentState != previousState)
        {
            Debug.Log($"[ArmyAI] State changed: {previousState} �� {currentState}");
            previousState = currentState;
        }

        // ���º� �ڵ鷯 ȣ��
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

    // ���� ��ȯ ���� �Ǵ�
    private void EvaluateState()
    {
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;        
        currentState = (hpRatio <= retreatHPThreshold) ? AIState.Retreat : AIState.Advance;
    }

    // Advance ���� �� ���� ����� ������ �̵�
    private void HandleAdvance()
    {
        armyStatus.HandleMovement();
    }

    // Retreat ���� �� ���� �������� ���� �Ǵ� ���
    private void HandleRetreat()
    {
        // �Ŵ������� ���� ����� ���� ���� ã��
        Transform targetPoint = RallyPointManager.Instance
            .GetNearestRetreatPoint(armyStatus.teamType, transform.position);

        if (targetPoint != null)
        {
            Vector3 dir = (targetPoint.position - transform.position).normalized;
            transform.position += dir * armyStatus.speed * Time.deltaTime;
        }
        else
        {
            // ����Ʈ�� ���� ��� ��� ���·� ��ȯ
            Debug.LogWarning("[ArmyAI] No retreat points found for team " + armyStatus.teamType);
            currentState = AIState.Regroup;
        }
    }


    // Regroup ���� �� �ֺ� ���� ����� �� ���� ��ȯ
    private void HandleRegroup()
    {
        // TODO: �ֺ� �Ʊ����� ���� ����� �� ���� �ൿ ����
    }

    // �ܺο��� ���� ���� ���� ����
    public void SetState(AIState newState)
    {
        currentState = newState;
    }
}
