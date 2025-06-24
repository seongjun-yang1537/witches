using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HomingMissile : MonoBehaviour
{
    [Header("미사일 설정")]
    public float speed = 10f;
    public float turnSpeed = 5f;
    public float maxLifetime = 10f;

    private JetStatus target;
    private ArmyStatus.TeamType ownerTeam;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
            return;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        // ✅ 추가: 일정 거리 이내로 접근하면 판정
        float proximityThreshold = 0.5f;
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget < proximityThreshold)
        {
            TryApplyDamage(target);
            Destroy(gameObject);
            return;
        }

        // 추적 이동
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;

        // 수명 초과 시 파괴
        if (Time.time - spawnTime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        // 자기 자신이 발사한 팀의 Jet에는 영향 없음
        JetStatus hitJet = other.GetComponent<JetStatus>();
        if (hitJet != null && hitJet.teamType != ConvertTeam(ownerTeam))
        {
            TryApplyDamage(hitJet);
            Destroy(gameObject);
        }
    }

    public void SetTarget(JetStatus jet)
    {
        target = jet;
    }

    public void SetOwnerTeam(ArmyStatus.TeamType team)
    {
        ownerTeam = team;
    }

    void TryApplyDamage(JetStatus jet)
    {
        float hitChance = 0.5f;
        // ✅ EW 기체는 명중률 하향
        if (jet.jetType == JetStatus.JetType.ElectronicWarfare)
            hitChance = 0.1f;

        bool isHit = Random.value <= hitChance;

        if (isHit)
        {
            Debug.Log("[Missile] 명중! CombatResultPopup 호출");
            CombatResultHandler.Instance?.HandleAntiAirHit(jet);
        }
        else
        {
            Debug.Log("[Missile] 회피 성공 - CombatResultPopup 호출");

            CombatResultHandler.Instance?.HandleAntiAirMiss(jet);
        }
    }


    private JetStatus.TeamType ConvertTeam(ArmyStatus.TeamType team)
    {
        return team switch
        {
            ArmyStatus.TeamType.Blue => JetStatus.TeamType.Blue,
            ArmyStatus.TeamType.Red => JetStatus.TeamType.Red,
            _ => JetStatus.TeamType.Blue
        };
    }

}
