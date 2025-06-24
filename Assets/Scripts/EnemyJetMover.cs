using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class EnemyJetMover : MonoBehaviour
{
    public float patrolRadius = 3f;         // ���� ��ȸ �ݰ�
    public float patrolSpeed = 1f;          // ��ȸ �ӵ�
    public float chaseSpeed = 4f;           // �߰� �ӵ�
    public float detectionRange = 15f;      // Ž�� �ݰ�
    public float contactRadius = 1f;        // �浹 ���� ����
    public float checkInterval = 1.0f;      // Ž�� �ֱ�

    public LayerMask targetLayer;           // Blue Jet Ž����

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

        if (targetJet == null)
        {
            Patrol();
            checkTimer -= Time.deltaTime;
            if (checkTimer <= 0f)
            {
                checkTimer = checkInterval;
                FindTargetJet();
            }
        }
        else
        {
            Chase();
            TryCombat();
        }
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
        Debug.Log($"[EnemyJet] Ž���� ��� ��: {hits.Length}");

        float closestDist = float.MaxValue;
        JetStatus closestJet = null;

        foreach (var hit in hits)
        {
            JetStatus jet = hit.GetComponent<JetStatus>();
            if (jet != null && jet.teamType == JetStatus.TeamType.Blue && !jet.isHealing)
            {
                float dist = Vector3.Distance(transform.position, jet.transform.position);
                Debug.Log($"[EnemyJet] ������ {jet.title}, �Ÿ�: {dist}");

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestJet = jet;
                }
            }
        }

        if (closestJet != null)
        {
            Debug.Log($"[EnemyJet] Ÿ�� ����: {closestJet.title}");
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

            // �浹 �� �ٽ� ��� ���·� ����
            targetJet = null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
