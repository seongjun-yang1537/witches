using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 양측 동시 1회 공격 방식 전투 처리 핸들러
/// - 양측 모두 기본 데미지 × 상성 배율
/// - 피해량 및 파괴 여부 출력
/// </summary>
public class CombatResultHandler : MonoBehaviour
{
    public static CombatResultHandler Instance { get; private set; }

    [Header("전투 결과 팝업")]
    public CombatResultPopup resultPopup;

    [Header("기본 데미지 설정")]
    public float baseDamageJet = 50f;
    public float baseDamageGround = 40f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void HandleCombat(JetStatus jet, ArmyStatus target)
    {
        if (jet == null || target == null) return;

        PrototypeGameManager.Instance?.PauseGameplay();

        var jetType = jet.GetCombatUnitType();
        var targetType = target.GetCombatUnitType();

        float jetToTargetMultiplier = UnitAffinityManager.Instance.GetMultiplier(jetType, targetType);
        float targetToJetMultiplier = UnitAffinityManager.Instance.GetMultiplier(targetType, jetType);

        float damageToTarget = baseDamageJet * jetToTargetMultiplier;
        float damageToJet = baseDamageGround * targetToJetMultiplier;

        target.currentHP -= damageToTarget;
        jet.currentHP -= damageToJet;

        bool targetDestroyed = target.currentHP <= 0f;
        bool jetDestroyed = jet.currentHP <= 0f;

        // ✅ 색상 라벨 적용
        string jetLabel = ColoredName(jet.title, jet.teamType == JetStatus.TeamType.Blue ? "blue" : "red");
        string targetLabel = ColoredName(target.title, target.teamType == ArmyStatus.TeamType.Blue ? "blue" : "red");

        // ✅ 로그 구성
        List<string> logLines = new List<string>
    {
        $"{jetLabel} dealt {damageToTarget:F0} damage to {targetLabel} (HP: {Mathf.Max(0, Mathf.FloorToInt(target.currentHP))})",
        $"{targetLabel} dealt {damageToJet:F0} damage to {jetLabel} (HP: {Mathf.Max(0, Mathf.FloorToInt(jet.currentHP))})"
    };

        if (jetDestroyed) logLines.Add($"{jetLabel} has been destroyed");
        if (targetDestroyed) logLines.Add($"{targetLabel} has been destroyed");

        resultPopup.gameObject.SetActive(true);
        resultPopup.ShowResult(logLines, () =>
        {
            PrototypeGameManager.Instance?.ResumeGameplay();

            if (jetDestroyed)
                Destroy(jet.gameObject);
            else
                jet.GetComponent<JetMover>()?.BeginReturn();

            if (targetDestroyed)
                Destroy(target.gameObject);
        });
    }

    /// <summary>
    /// 이름을 팀 색상으로 감싸는 유틸
    /// </summary>
    private string ColoredName(string name, string team)
    {
        return team == "blue" ? $"<color=#5080FF>{name}</color>"
             : team == "red" ? $"<color=#FF5050>{name}</color>"
             : name;
    }
}
