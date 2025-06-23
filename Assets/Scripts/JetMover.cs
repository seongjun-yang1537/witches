using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class JetMover : MonoBehaviour
{
    [Header("이동 및 전투 설정")]
    public float speed = 4f;
    public float contactRadius = 1.0f;
    public float stopDistance = 0.6f;
    public LayerMask enemyLayer;

    [Header("귀환 관련")]
    public Vector3 homePosition;
    public AirbaseItemUI originItemUI;
    public System.Action onReturnComplete;

    private BoxCollider boxCollider;
    private bool isReturning = false;
    private readonly List<ArmyStatus> inContact = new();
    private Transform targetOverride = null;
    private bool hasFought = false; // ✅ 중복 전투 방지용 플래그

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

        // ✅ 게임 일시정지 시 Update 정지
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        if (isReturning)
        {
            ReturnToBase();
            return;
        }

        MoveTowardTarget();
        HandleCombat(); // ✅ 전투 감지 및 처리
    }

    /// <summary>
    /// 외부에서 타겟 지정
    /// </summary>
    public void SetTarget(Transform target)
    {
        targetOverride = target;
    }

    /// <summary>
    /// 타겟을 향해 이동
    /// </summary>
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

    /// <summary>
    /// 적과 충돌 시 전투 처리
    /// </summary>
    void HandleCombat()
    {
        if (hasFought) return; // ✅ 이미 전투한 경우 중복 방지

        Collider[] hits = Physics.OverlapSphere(transform.position, contactRadius, enemyLayer);

        foreach (var hit in hits)
        {
            ArmyStatus enemy = hit.GetComponent<ArmyStatus>();
            if (enemy != null)
            {
                // ✅ 전투 처리 핸들러 호출
                var myStatus = GetComponent<JetStatus>();
                if (myStatus != null)
                {
                    hasFought = true; // 중복 전투 방지 설정
                    CombatResultHandler.Instance.HandleCombat(myStatus, enemy);
                }

                // ✅ 귀환 조건은 CombatResultHandler 에서 결정
                break;
            }
        }
    }

    /// <summary>
    /// 전투기 기지로 귀환 처리
    /// </summary>
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

            // UI 항목 복구
            if (originItemUI != null)
            {
                originItemUI.SetAvailable(true);
                Debug.Log("[JetMover] UI 항목 복구 완료");
            }

            onReturnComplete?.Invoke();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 전투 후 생존 시 외부에서 호출되는 귀환 명령
    /// </summary>
    public void BeginReturn()
    {
        isReturning = true;
        targetOverride = null;
    }
}
