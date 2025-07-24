// ArmyAIController.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("AI ����")]
    public AIState currentState = AIState.Advance;
    [Range(0f, 1f)] public float retreatHPThreshold = 0.75f;

    [Header("�� ���� �ݰ�")]
    public float detectionRadius = 2f;

    [Header("���漱 ����")]
    [Tooltip("���漱���κ��� �󸶳� �Ѿ�� ��ǥ�� ���� ������ �Ÿ�")]
    public float borderCrossLimit = 1f;

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

        // Healing ���� �� ó�� (���� ����)
        if (isInCityTrigger && armyStatus.currentHP < armyStatus.maxHP && currentState != AIState.Healing)
        {
            armyStatus.StopMovement();
            currentState = AIState.Healing;
        }
        if (currentState == AIState.Healing)
        {
            HandleHealing();
            LogStateChange();
            return;
        }

        // HP ���� Advance/Retreat
        float hpRatio = armyStatus.currentHP / armyStatus.maxHP;
        AIState newState = (hpRatio <= retreatHPThreshold) ? AIState.Retreat : AIState.Advance;
        if (newState != currentState)
        {
            currentState = newState;
            if (currentState == AIState.Retreat)
            {
                string hex = ColorUtility.ToHtmlStringRGB(armyStatus.TeamColor);
                string coloredName = $"<color=#{hex}>{armyStatus.title}</color>";
                WarlogManager.Instance.LogEvent(coloredName, "Start Retreat");
            }
        }
        LogStateChange();

        // ���º� �ൿ
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
            Debug.Log($"[ArmyAI] State changed: {previousState} �� {currentState}");
            previousState = currentState;
        }
    }

    private bool IsBeyondBorder(Vector3 pos)
    {
        float borderZ = BorderLineDrawer.currentBorderZ;
        if (armyStatus.teamType == ArmyStatus.TeamType.Red)
        {
            // Red�� Z > borderZ ����, borderZ ���Ϸ� �󸶳� �������� ����
            return (borderZ - pos.z) > borderCrossLimit;
        }
        else
        {
            // Blue�� Z < borderZ ����, borderZ �̻����� �󸶳� �Ѿ�� ����
            return (pos.z - borderZ) > borderCrossLimit;
        }
    }

    void HandleAdvance()
    {
        // 1) �ٰŸ� �� ���� ����
        var hits = Physics.OverlapSphere(transform.position, detectionRadius, armyStatus.enemyLayer);
        ArmyStatus closest = null; float minD = float.MaxValue;
        foreach (var col in hits)
        {
            var e = col.GetComponent<ArmyStatus>();
            if (e == null) continue;
            if (IsBeyondBorder(e.transform.position)) continue;  // ���漱 ���� �� ����
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minD) { minD = d; closest = e; }
        }
        if (closest != null)
        {
            armyStatus.ResumeMovement();
            armyStatus.MoveTo(closest.transform.position);
            return;
        }

        // 2) �� ���÷� �̵�
        CityStatus nearestCity = null; float minDist = float.MaxValue;
        foreach (var city in FindObjectsOfType<CityStatus>())
        {
            if (city.owner == armyStatus.teamType) continue;
            if (IsBeyondBorder(city.transform.position)) continue;  // ���漱 ���� ���� ����
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
        var hits = Physics.OverlapSphere(transform.position, detectionRadius, armyStatus.enemyLayer);
        if (hits.Length > 0)
        {
            currentState = AIState.Advance;
            return;
        }
        armyStatus.currentHP += armyStatus.healPerSecond * Time.deltaTime;
        if (armyStatus.currentHP >= armyStatus.maxHP)
        {
            armyStatus.currentHP = armyStatus.maxHP;
            isInCityTrigger = false;
            armyStatus.ResumeMovement();
            currentState = AIState.Advance;
            HandleAdvance();
        }
    }

    void HandleRegroup()
    {
        // TODO
    }

    void OnTriggerEnter(Collider other)
    {
        var city = other.GetComponent<CityStatus>();
        if (city != null && city.owner == armyStatus.teamType)
        {
            isInCityTrigger = true;
            armyStatus.StopMovement();
            currentState = AIState.Healing;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var city = other.GetComponent<CityStatus>();
        if (city != null && city.owner == armyStatus.teamType)
            isInCityTrigger = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
