using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class EnemyJetMover : MonoBehaviour
{
    public float patrolRadius = 3f;         // 원형 선회 반경
    public float patrolSpeed = 1f;          // 선회 속도
    public float chaseSpeed = 4f;           // 추격 속도
    public float detectionRange = 15f;      // 탐지 반경
    public float contactRadius = 1f;        // 충돌 판정 범위
    public float checkInterval = 1.0f;      // 탐색 주기

    public LayerMask targetLayer;           // Blue Jet 탐지용

    private Vector3 centerPosition;
    private float patrolAngle = 0f;
    private JetStatus selfStatus;
    private JetStatus targetJet;

    private float checkTimer = 0f;

    private BoxCollider boxCollider;

    void Start()
    {
        centerPosition = transform.position;
        selfStatus = GetComponent<JetStatus>();
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = Vector3.one;
        boxCollider.center = Vector3.zero;
    }

    void Update()
    {
        if (!Application.isPlaying || selfStatus == null || selfStatus.teamType != JetStatus.TeamType.Red)
            return;

        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        // ✅ 타겟이 유효하지 않으면 탐색 루틴으로 전환
        if (targetJet == null || !targetJet.gameObject.activeInHierarchy || targetJet.isHealing)
        {
            Patrol();
            checkTimer -= Time.deltaTime;
            if (checkTimer <= 0f)
            {
                checkTimer = checkInterval;
                FindTargetJet();
            }

            return; // ✅ 이 줄이 반드시 필요합니다!
        }

        // ✅ 유효한 타겟이면 추격 및 전투
        Chase();
        TryCombat();
    }

    void Patrol()
    {
        patrolAngle += patrolSpeed * Time.deltaTime;
        if (patrolAngle > 360f) patrolAngle -= 360f;

        float x = Mathf.Cos(patrolAngle) * patrolRadius;
        float z = Mathf.Sin(patrolAngle) * patrolRadius;

        Vector3 nextPos = centerPosition + new Vector3(x, 0, z);
        transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime * 2f);
    }

    void FindTargetJet()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);
        Debug.Log($"[EnemyJet] 탐색됨 대상 수: {hits.Length}");

        float closestDist = float.MaxValue;
        JetStatus closestJet = null;

        foreach (var hit in hits)
        {
            JetStatus jet = hit.GetComponent<JetStatus>();
            if (jet != null && jet.teamType == JetStatus.TeamType.Blue && !jet.isHealing)
            {
                float dist = Vector3.Distance(transform.position, jet.transform.position);
                Debug.Log($"[EnemyJet] 감지된 {jet.title}, 거리: {dist}");

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestJet = jet;
                }
            }
        }

        if (closestJet != null)
        {
            Debug.Log($"[EnemyJet] 타겟 지정: {closestJet.title}");
            targetJet = closestJet;
        }
    }


    void Chase()
    {
        if (targetJet == null || !targetJet.gameObject.activeInHierarchy)
        {
            targetJet = null;
            return;
        }

        Vector3 toTarget = targetJet.transform.position - transform.position;
        float dist = toTarget.magnitude;

        if (dist > 0.1f)
        {
            transform.position += toTarget.normalized * chaseSpeed * Time.deltaTime;
        }
    }

    void TryCombat()
    {
        if (targetJet == null) return;

        float dist = Vector3.Distance(transform.position, targetJet.transform.position);
        if (dist <= contactRadius)
        {
            CombatResultHandler.Instance?.HandleCombat(selfStatus, targetJet);

            // 충돌 후 다시 대기 상태로 복귀
            targetJet = null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
