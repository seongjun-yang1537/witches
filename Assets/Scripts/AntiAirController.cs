using UnityEngine;

[RequireComponent(typeof(ArmyStatus))]
public class AntiAirController : MonoBehaviour
{
    [Header("탐지 설정")]
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

            // 아군은 타겟에서 제외
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
            Debug.LogWarning("[AntiAir] 미사일 프리팹 또는 타겟이 없습니다.");
            return;
        }

        GameObject missile = Instantiate(missilePrefab, missileSpawnPoint.position, Quaternion.identity);
        HomingMissile homing = missile.GetComponent<HomingMissile>();
        if (homing != null)
        {
            homing.SetTarget(target);
            homing.SetOwnerTeam(selfStatus.teamType);
        }

        Debug.Log($"[AntiAir] {target.name} 에게 미사일 발사");
    }

    // Blue -> Red, Red -> Blue 변환
    ArmyStatus.TeamType ConvertTeamType(ArmyStatus.TeamType type)
    {
        return type == ArmyStatus.TeamType.Blue ? ArmyStatus.TeamType.Red : ArmyStatus.TeamType.Blue;
    }

    // AntiAirController.cs 내부 아무 곳에 (private로 선언)
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
