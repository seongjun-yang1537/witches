using UnityEngine;

[RequireComponent(typeof(ArmyStatus))]
public class AntiAirController : MonoBehaviour
{
    [Header("Ž�� ����")]
    public float detectionRadius = 10f;
    public LayerMask jetLayer;
    public GameObject missilePrefab;
    public Transform missileSpawnPoint;

    private float lastFireTime = -999f;
    private ArmyStatus selfStatus;

    void Start()
    {
        selfStatus = GetComponent<ArmyStatus>();

        if (missileSpawnPoint == null)
            missileSpawnPoint = this.transform;
    }

    void Update()
    {
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        if (!CanFire()) return;

        JetStatus target = FindNearestEnemyJet();
        if (target != null)
        {
            FireMissileAt(target);
            lastFireTime = Time.time;
        }
    }

    bool CanFire()
    {
        float reload = PrototypeGameManager.Instance?.antiAirReloadTime ?? 5f;
        return Time.time - lastFireTime >= reload;
    }

    JetStatus FindNearestEnemyJet()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, jetLayer);
        JetStatus closestJet = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            JetStatus jet = hit.GetComponent<JetStatus>();
            if (jet == null) continue;

            // �Ʊ��� Ÿ�ٿ��� ����
            if (ConvertTeam(jet.teamType) == selfStatus.teamType)
                continue;

            float dist = Vector3.Distance(transform.position, jet.transform.position);
            if (dist < closestDistance)
            {
                closestJet = jet;
                closestDistance = dist;
            }
        }

        return closestJet;
    }

    void FireMissileAt(JetStatus target)
    {
        if (missilePrefab == null || target == null)
        {
            Debug.LogWarning("[AntiAir] �̻��� ������ �Ǵ� Ÿ���� �����ϴ�.");
            return;
        }

        GameObject missile = Instantiate(missilePrefab, missileSpawnPoint.position, Quaternion.identity);
        HomingMissile homing = missile.GetComponent<HomingMissile>();
        if (homing != null)
        {
            homing.SetTarget(target);
            homing.SetOwnerTeam(selfStatus.teamType);
        }

        Debug.Log($"[AntiAir] {target.name} ���� �̻��� �߻�");
    }

    // Blue -> Red, Red -> Blue ��ȯ
    ArmyStatus.TeamType ConvertTeamType(ArmyStatus.TeamType type)
    {
        return type == ArmyStatus.TeamType.Blue ? ArmyStatus.TeamType.Red : ArmyStatus.TeamType.Blue;
    }

    // AntiAirController.cs ���� �ƹ� ���� (private�� ����)
    private ArmyStatus.TeamType ConvertTeam(JetStatus.TeamType jetTeam)
    {
        return jetTeam switch
        {
            JetStatus.TeamType.Blue => ArmyStatus.TeamType.Blue,
            JetStatus.TeamType.Red => ArmyStatus.TeamType.Red,
            _ => ArmyStatus.TeamType.Blue
        };
    }

}
