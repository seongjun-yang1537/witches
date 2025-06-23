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

    public void ResetForNewDeployment()
    {
        isReturning = false;
        targetOverride = null;
        inContact.Clear();

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        var visual = transform.Find("VisualRoot");
        if (visual != null) visual.gameObject.SetActive(true);
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
        Collider[] hits = Physics.OverlapSphere(transform.position, contactRadius, enemyLayer);

        foreach (var hit in hits)
        {
            ArmyStatus enemy = hit.GetComponent<ArmyStatus>();
            if (enemy != null)
            {
                var myStatus = GetComponent<JetStatus>();
                if (myStatus != null)
                {
                    CombatResultHandler.Instance.HandleCombat(myStatus, enemy);
                }

                isReturning = true;
                targetOverride = null;
                break;
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
            onReturnComplete?.Invoke();

            var status = GetComponent<JetStatus>();
            if (status != null)
                status.StartHealing();

            enabled = false;

            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            var visual = transform.Find("VisualRoot");
            if (visual != null) visual.gameObject.SetActive(false);
        }
    }
    public void BeginReturn()
    {
        isReturning = true;
        targetOverride = null;
    }
}
