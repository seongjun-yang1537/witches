// ✅ ArmyMover.cs - 다대다 전투 + 가장 가까운 적 추적 구조 + 디버그 로그 추가 (회전 제거됨 + 충돌 시 정지 개선 + 거리 기준 정지)

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

        // ✅ 이동 및 충돌 거리 확인 (전체 거리 기준으로 변경)
        if (closestTarget != null)
        {
            Vector3 toTarget = closestTarget.transform.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance > stopDistance)
            {
                Debug.DrawLine(transform.position, closestTarget.transform.position, Color.red);
                Debug.Log($"[{gameObject.name}] Moving to: {closestTarget.gameObject.name} ({distance:F2}m)");

                Vector3 dir = toTarget.normalized;
                transform.position += dir * speed * Time.deltaTime;
            }
            else
            {
                Debug.Log($"[{gameObject.name}] Stopped near {closestTarget.name} (distance {distance:F2}m)");
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] No target found in enemyLayer: {enemyLayer.value}");
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

        // ✅ 더 이상 접촉하지 않는 유닛 정리
        for (int i = inContact.Count - 1; i >= 0; i--)
        {
            var target = inContact[i];
            if (!currentContacts.Contains(target))
            {
                target.RemoveAttacker(selfStatus);
                inContact.RemoveAt(i);
            }
        }
    }

    void UpdateBoxColliderSize()
    {
        if (boxCollider == null) return;
        boxCollider.size = Vector3.one;
        boxCollider.center = Vector3.zero;
    }
}
