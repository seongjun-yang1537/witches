using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class JetMover : MonoBehaviour
{
    public float speed = 4f;
    public float contactRadius = 1.0f;
    public float stopDistance = 0.6f;
    public LayerMask enemyLayer;

    public Vector3 homePosition;
    public AirbaseItemUI originItemUI;

    private BoxCollider boxCollider;
    private bool isReturning = false;
    private readonly List<ArmyStatus> inContact = new();
    private Transform targetOverride = null;

    public System.Action onReturnComplete;

    void OnEnable()
    {
        Debug.Log("[JetMover] OnEnable 호출됨");

        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = Vector3.one;
        boxCollider.center = Vector3.zero;
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        if (isReturning)
        {
            ReturnToBase();
            return;
        }

        MoveTowardTarget();
        HandleCombat();
    }

    public void SetTarget(Transform target)
    {
        targetOverride = target;
    }

    void MoveTowardTarget()
    {
        if (targetOverride == null) return;

        float dist = Vector3.Distance(transform.position, targetOverride.position);
        if (dist > stopDistance)
        {
            Vector3 dir = (targetOverride.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
    }

    void HandleCombat()
    {
        HashSet<ArmyStatus> currentContacts = new();
        Collider[] hits = Physics.OverlapSphere(transform.position, contactRadius, enemyLayer);

        foreach (var hit in hits)
        {
            ArmyStatus other = hit.GetComponent<ArmyStatus>();
            if (other != null)
            {
                currentContacts.Add(other);

                if (!inContact.Contains(other))
                {
                    // ArmyStatus에 대한 공격자 등록 생략
                    inContact.Add(other);

                    // ✅ 첫 충돌 시 귀환 모드 전환
                    isReturning = true;
                    targetOverride = null;
                }
            }
        }

        // 접촉이 끝난 대상 제거
        for (int i = inContact.Count - 1; i >= 0; i--)
        {
            var target = inContact[i];
            if (!currentContacts.Contains(target))
            {
                // ArmyStatus 제거 생략
                inContact.RemoveAt(i);
            }
        }
    }

    void ReturnToBase()
    {
        Vector3 toHome = homePosition - transform.position;
        float distance = toHome.magnitude;

        if (distance > stopDistance)
        {
            transform.position += toHome.normalized * speed * Time.deltaTime;
        }
        else
        {
            Debug.Log("[JetMover] 귀환 완료");

            if (originItemUI != null)
            {
                originItemUI.SetAvailable(true);
                Debug.Log("[JetMover] UI 항목 복구 완료");
            }

            onReturnComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}
