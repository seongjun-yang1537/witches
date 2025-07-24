using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("회복 설정")]
    [Tooltip("도시에 도착했을 때 초당 회복량")]
    public float healPerSecond = 10f;

    [Header("아군 충돌 방지")]
    public LayerMask allyLayer;
    public float allyAvoidDistance = 0.5f;
    public float allyPushStrength = 0.5f;

    [Header("UI 설정")]
    public GameObject uiPrefab;
    public Canvas uiCanvas;

    [Header("상태 정보")]
    public float currentHP;

    // 여기에 팀별 색상을 세팅해 둡니다.
    public Color TeamColor => teamType == TeamType.Red ? Color.red : Color.blue;

    // 내부 컴포넌트
    private BoxCollider boxCollider;
    private GameObject createdUI;
    private readonly List<ArmyStatus> attackers = new List<ArmyStatus>();
    private readonly List<ArmyStatus> inContact = new List<ArmyStatus>();
    private NavMeshAgent agent;

    void OnValidate()
    {
        // teamType에 따라 자동으로 레이어 마스크 설정
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

    void Awake()
    {
        // NavMeshAgent 컴포넌트 초기화 및 파라미터 동기화
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
            agent.stoppingDistance = stopDistance;
            agent.autoBraking = false;
        }
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

        // 매 프레임 전투, 회피, UI 업데이트
        UpdateBoxColliderSize();
        HandleCombatDetection();
        HandleAllyAvoidance();
        UpdateUILabel();
    }

    /// <summary>
    /// NavMeshAgent를 이용해 목적지로 이동합니다.
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (agent == null) return;
        agent.SetDestination(destination);
    }

    /// <summary>
    /// NavMeshAgent 기반으로 가장 가까운 적을 찾아 추적 이동합니다.
    /// </summary>
    public void HandleMovement()
    {
        if (agent == null) return;

        // 가장 가까운 적 찾기
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
            float distance = Vector3.Distance(transform.position, closest.transform.position);
            if (distance > stopDistance)
            {
                agent.SetDestination(closest.transform.position);
            }
            else
            {
                if (agent.hasPath)
                    agent.ResetPath();
            }
        }
        else
        {
            if (agent.hasPath)
                agent.ResetPath();
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
                    // 적 유닛과 최초 접촉 시점
                    inContact.Add(other);

                    // 중복 로그 방지를 위해, 팀 Blue만 또는 이름 순으로 호출
                    if (teamType == TeamType.Blue || string.Compare(title, other.title) < 0)
                    {
                        // Rich Text를 활용해 팀색 넣기
                        string hexA = ColorUtility.ToHtmlStringRGB(TeamColor);
                        string hexB = ColorUtility.ToHtmlStringRGB(other.TeamColor);
                        string nameA = $"<color=#{hexA}>{title}</color>";
                        string nameB = $"<color=#{hexB}>{other.title}</color>";

                        // “양측 ↔ Combat Started” 형식으로 로그
                        WarlogManager.Instance.LogEvent(
                            $"{nameA} ↔ {nameB}",
                            "Combat Started"
                        );
                    }

                    // 기존 공격자 등록
                    other.AddAttacker(this);
                }
            }
        }
        // 접촉이 끝난 적은 리스트에서 제거
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
                // 유닛 파괴 직전 Warlog 기록
                if (currentHP <= 0f)
                {
                    // 팀 색상으로 유닛명 감싸기
                    string hex = ColorUtility.ToHtmlStringRGB(TeamColor);
                    string coloredName = $"<color=#{hex}>{title}</color>";
                    // “Destroyed” 로그 1회 남기기
                    WarlogManager.Instance.LogEvent(coloredName, "<b>Destroyed</b>");

                    if (createdUI != null)
                        Destroy(createdUI);
                    Destroy(gameObject);
                    yield break;
                }
            }

            var wait = PrototypeGameManager.Instance?.armyCombatTickInterval ?? 0.5f;
            yield return new WaitForSeconds(wait);
        }
    }


    #region Public API
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
    #endregion

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

    public void StopMovement()
    {
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    public void ResumeMovement()
    {
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }



    void OnDestroy()
    {
        GameStateManager.Instance?.UnregisterArmy(this);
    }

}
