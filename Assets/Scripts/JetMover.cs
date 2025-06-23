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
    public ArmyStatus selfStatus;

    public System.Action onReturnComplete;

    public AirbaseItemUI originItemUI;  // 출격 명령을 내린 UI 아이템


    private BoxCollider boxCollider;
    private bool isReturning = false;
    private readonly List<ArmyStatus> inContact = new();

    private Transform targetOverride;  // 추가

    public void SetTarget(Transform target)
    {
        targetOverride = target;
    }


    void OnEnable()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = Vector3.one;
        boxCollider.center = Vector3.zero;
    }

    void Update()
    {
        if (!Application.isPlaying || selfStatus == null) return;

        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        if (isReturning)
        {
            ReturnToBase();
            return;
        }

            // Update 내부에서 FindClosestEnemy 대신 아래로 교체
        if (targetOverride != null)
        {
            float dist = Vector3.Distance(transform.position, targetOverride.position);
            if (dist > stopDistance)
            {
                Vector3 dir = (targetOverride.position - transform.position).normalized;
                transform.position += dir * speed * Time.deltaTime;
            }
        }

        HandleCombat();
    }


    void HandleCombat()
    {
        HashSet<ArmyStatus> currentContacts = new();
        Collider[] hits = Physics.OverlapSphere(transform.position, contactRadius, enemyLayer);

        foreach (var hit in hits)
        {
            ArmyStatus other = hit.GetComponent<ArmyStatus>();
            if (other != null && other != selfStatus)
            {
                currentContacts.Add(other);

                if (!inContact.Contains(other))
                {
                    other.AddAttacker(selfStatus);
                    inContact.Add(other);

                    // ✅ 첫 충돌 시 귀환 시작
                    isReturning = true;
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
    }

    void ReturnToBase()
    {
        if (homePosition == null) return;

        Vector3 toHome = homePosition - transform.position;
        float distance = toHome.magnitude;

        if (distance > stopDistance)
        {
            transform.position += toHome.normalized * speed * Time.deltaTime;
        }
        else
        {
            if (originItemUI != null)
            {
                Debug.Log("[JetMover] 복귀 완료 - UI 활성화 시도");
                originItemUI.SetAvailable(true);
            }
            else
            {
                Debug.LogWarning("[JetMover] originItemUI is null!");
            }

            Destroy(gameObject);
        }
    }
}
