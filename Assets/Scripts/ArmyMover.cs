// ✅ ArmyMover.cs - 다대다 전투 + 가장 가까운 적 추적 + 아군 간 겹침 방지

using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
public class ArmyMover : MonoBehaviour
{
    public float speed = 2f;
    public float contactRadius = 1.0f;               // 충돌 거리 기준
    public float stopDistance = 0.6f;                // 정지 거리 기준
    public LayerMask enemyLayer;                     // 적 유닛 탐지용 레이어

    [Header("아군 충돌 방지")]
    public LayerMask allyLayer;                      // 아군 탐지용 레이어
    public float allyAvoidDistance = 0.5f;           // 아군 최소 거리
    public float allyPushStrength = 0.5f;            // 아군 밀어내기 강도

    public ArmyStatus selfStatus;
    private BoxCollider boxCollider;

    private readonly List<ArmyStatus> inContact = new List<ArmyStatus>();

    void OnEnable()
    {
        boxCollider = GetComponent<BoxCollider>();
        UpdateBoxColliderSize();
    }

    void Update()
    {
        if (!Application.isPlaying || selfStatus == null) return;

        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        UpdateBoxColliderSize();

        // ✅ 가장 가까운 적 유닛 탐색
        ArmyStatus closestTarget = null;
        float minDistance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(transform.position, 20f, enemyLayer);

        foreach (var hit in hits)
        {
            ArmyStatus other = hit.GetComponent<ArmyStatus>();
            if (other != null && other != selfStatus)
            {
                float dist = Vector3.Distance(transform.position, other.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTarget = other;
                }
            }
        }

        // ✅ 이동 및 정지 처리
        if (closestTarget != null)
        {
            Vector3 toTarget = closestTarget.transform.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance > stopDistance)
            {
                Vector3 dir = toTarget.normalized;
                transform.position += dir * speed * Time.deltaTime;
            }
        }

        // ✅ 충돌 판정
        HashSet<ArmyStatus> currentContacts = new HashSet<ArmyStatus>();
        Collider[] contactHits = Physics.OverlapSphere(transform.position, contactRadius, enemyLayer);

        foreach (var hit in contactHits)
        {
            ArmyStatus other = hit.GetComponent<ArmyStatus>();
            if (other != null && other != selfStatus)
            {
                currentContacts.Add(other);

                if (!inContact.Contains(other))
                {
                    other.AddAttacker(selfStatus);
                    inContact.Add(other);
                }
            }
        }

        for (int i = inContact.Count - 1; i >= 0; i--)
        {
            var target = inContact[i];
            if (!currentContacts.Contains(target))
            {
                target.RemoveAttacker(selfStatus);
                inContact.RemoveAt(i);
            }
        }

        // ✅ 아군 간 밀어내기 처리
        Collider[] allyHits = Physics.OverlapSphere(transform.position, allyAvoidDistance, allyLayer);

        foreach (var hit in allyHits)
        {
            if (hit.transform == transform) continue;

            Vector3 pushDir = transform.position - hit.transform.position;
            pushDir.y = 0f;
            if (pushDir.sqrMagnitude < 0.001f) continue;

            transform.position += pushDir.normalized * allyPushStrength * Time.deltaTime;
        }
    }

    void UpdateBoxColliderSize()
    {
        if (boxCollider == null) return;
        boxCollider.size = Vector3.one;
        boxCollider.center = Vector3.zero;
    }
}
