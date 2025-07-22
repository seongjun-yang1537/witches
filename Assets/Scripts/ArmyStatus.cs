using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArmyStatus : MonoBehaviour
{
    public enum TeamType { Blue, Red }
    public enum UnitType { Armor, Infantry, AntiAir }

    [Header("기본 정보")]
    public TeamType teamType;
    public UnitType unitType;
    public string title = "Unit";
    public float maxHP = 100f;

    [Header("이동 설정")]
    public float speed = 2f;
    public float contactRadius = 1.0f;
    public float stopDistance = 0.6f;
    public LayerMask enemyLayer;

    [Header("아군 충돌 방지")]
    public LayerMask allyLayer;
    public float allyAvoidDistance = 0.5f;
    public float allyPushStrength = 0.5f;

    [Header("UI 설정")]
    public GameObject uiPrefab;
    public Canvas uiCanvas;

    [Header("상태 정보")]
    public float currentHP;

    private BoxCollider boxCollider;
    private GameObject createdUI;
    private readonly List<ArmyStatus> attackers = new List<ArmyStatus>();
    private readonly List<ArmyStatus> inContact = new List<ArmyStatus>();

    void OnValidate()
    {
        // teamType에 따라 적/아군 레이어를 자동 설정
        string allyName = teamType.ToString();
        string enemyName = (teamType == TeamType.Blue) ? TeamType.Red.ToString() : TeamType.Blue.ToString();
        allyLayer = LayerMask.GetMask(allyName);
        enemyLayer = LayerMask.GetMask(enemyName);
    }

    void OnEnable()
    {
        boxCollider = GetComponent<BoxCollider>();
        UpdateBoxColliderSize();
    }

    void Start()
    {
        currentHP = maxHP;
        GameStateManager.Instance?.RegisterArmy(this);
        SetupUI();
        StartCoroutine(DamageTickLoop());
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused) return;

        UpdateBoxColliderSize();
        HandleCombatDetection();
        HandleAllyAvoidance();
        UpdateUILabel();
    }

    public void HandleMovement()
    {
        ArmyStatus closest = null;
        float minDist = float.MaxValue;
        foreach (var hit in Physics.OverlapSphere(transform.position, 20f, enemyLayer))
        {
            var other = hit.GetComponent<ArmyStatus>();
            if (other != null && other != this)
            {
                float dist = Vector3.Distance(transform.position, other.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = other;
                }
            }
        }
        if (closest != null)
        {
            var toTarget = closest.transform.position - transform.position;
            if (toTarget.magnitude > stopDistance)
                transform.position += toTarget.normalized * speed * Time.deltaTime;
        }
    }

    private void HandleCombatDetection()
    {
        var current = new HashSet<ArmyStatus>();
        foreach (var hit in Physics.OverlapSphere(transform.position, contactRadius, enemyLayer))
        {
            var other = hit.GetComponent<ArmyStatus>();
            if (other != null && other != this)
            {
                current.Add(other);
                if (!inContact.Contains(other))
                {
                    other.AddAttacker(this);
                    inContact.Add(other);
                }
            }
        }
        for (int i = inContact.Count - 1; i >= 0; i--)
        {
            var oc = inContact[i];
            if (!current.Contains(oc))
            {
                oc.RemoveAttacker(this);
                inContact.RemoveAt(i);
            }
        }
    }

    private void HandleAllyAvoidance()
    {
        foreach (var hit in Physics.OverlapSphere(transform.position, allyAvoidDistance, allyLayer))
        {
            if (hit.transform == transform) continue;
            var dir = transform.position - hit.transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.001f) continue;
            transform.position += dir.normalized * allyPushStrength * Time.deltaTime;
        }
    }

    private void SetupUI()
    {
        if (uiPrefab == null || uiCanvas == null) return;
        createdUI = Instantiate(uiPrefab, uiCanvas.transform);
        var follower = createdUI.GetComponent<WorldTextFollower>();
        var tmp = createdUI.GetComponent<TextMeshProUGUI>();
        if (follower != null && tmp != null)
        {
            follower.target = transform;
            follower.status = this;
            follower.uiText = tmp;
            follower.label = title;
            var depth = transform.localScale.z;
            var distance = depth * 0.5f + 0.5f;
            var dir = (teamType == TeamType.Red) ? transform.forward : -transform.forward;
            follower.offset = dir * distance;
            tmp.alignment = (teamType == TeamType.Red) ?
                TextAlignmentOptions.TopGeoAligned : TextAlignmentOptions.BottomGeoAligned;
        }
    }

    private IEnumerator DamageTickLoop()
    {
        while (true)
        {
            if (PrototypeGameManager.Instance != null && PrototypeGameManager.Instance.IsGameplayPaused)
                yield return null;

            if (attackers.Count > 0)
            {
                foreach (var at in attackers)
                {
                    if (at == null) continue;
                    var randomFactor = Random.Range(0.95f, 1.05f);
                    var typeMul = UnitAffinityManager.Instance
                        .GetMultiplier(at.GetCombatUnitType(), GetCombatUnitType());
                    var tick = PrototypeGameManager.Instance.armyCombatTickInterval;
                    var baseDPS = PrototypeGameManager.Instance?.armyBaseDamagePerSecond ?? 10f;
                    currentHP -= baseDPS * typeMul * randomFactor * tick;
                }
                if (currentHP <= 0f)
                {
                    if (createdUI != null) Destroy(createdUI);
                    Destroy(gameObject);
                    yield break;
                }
            }
            var wait = PrototypeGameManager.Instance?.armyCombatTickInterval ?? 0.5f;
            yield return new WaitForSeconds(wait);
        }
    }

    // === 공용 API ===
    public void AddAttacker(ArmyStatus attacker)
    {
        if (attacker != null && !attackers.Contains(attacker))
            attackers.Add(attacker);
    }
    public void RemoveAttacker(ArmyStatus attacker)
    {
        if (attacker != null)
            attackers.Remove(attacker);
    }
    public int GetHPInt() => Mathf.FloorToInt(currentHP);
    public CombatUnitType GetCombatUnitType()
    {
        return unitType switch
        {
            UnitType.Infantry => CombatUnitType.Infantry,
            UnitType.Armor => CombatUnitType.Armor,
            UnitType.AntiAir => CombatUnitType.AntiAir,
            _ => CombatUnitType.Infantry
        };
    }

    private void UpdateUILabel()
    {
        if (createdUI == null) return;
        var tmp = createdUI.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = $"{title} {GetHPInt()}";
    }

    private void UpdateBoxColliderSize()
    {
        if (boxCollider == null) return;
        boxCollider.size = Vector3.one;
        boxCollider.center = Vector3.zero;
    }

    void OnDestroy()
    {
        GameStateManager.Instance?.UnregisterArmy(this);
    }
}
