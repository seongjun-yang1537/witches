using System.Collections.Generic;
using UnityEngine;

public class CombatResultHandler : MonoBehaviour
{
    public static CombatResultHandler Instance { get; private set; }

    [Header("전투 결과 팝업")]
    public CombatResultPopup resultPopup;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ✅ Jet vs Ground
    public void HandleCombat(JetStatus jet, ArmyStatus target)
    {
        if (jet == null || target == null) return;

        PrototypeGameManager.Instance?.PauseGameplay();

        var jetType = jet.GetCombatUnitType();
        var targetType = target.GetCombatUnitType();

        float baseDamageJet = PrototypeGameManager.Instance?.baseDamageJet ?? 50f;
        float baseDamageGround = PrototypeGameManager.Instance?.baseDamageGround ?? 40f;

        float jetToTargetMultiplier = UnitAffinityManager.Instance.GetMultiplier(jetType, targetType);
        float targetToJetMultiplier = UnitAffinityManager.Instance.GetMultiplier(targetType, jetType);

        // ✅ 랜덤 ±10% 반영
        float damageToTarget = baseDamageJet * jetToTargetMultiplier * Random.Range(0.9f, 1.1f);
        float damageToJet = baseDamageGround * targetToJetMultiplier * Random.Range(0.9f, 1.1f);

        target.currentHP -= damageToTarget;
        jet.currentHP -= damageToJet;

        bool targetDestroyed = target.currentHP <= 0f;
        bool jetDestroyed = jet.currentHP <= 0f;

        string jetLabel = ColoredName(jet.title, jet.teamType == JetStatus.TeamType.Blue ? "blue" : "red");
        string targetLabel = ColoredName(target.title, target.teamType == ArmyStatus.TeamType.Blue ? "blue" : "red");

        List<string> logLines = new()
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

    // ✅ Jet vs Jet
    public void HandleCombat(JetStatus jetA, JetStatus jetB)
    {
        if (jetA == null || jetB == null) return;

        PrototypeGameManager.Instance?.PauseGameplay();

        var typeA = jetA.GetCombatUnitType();
        var typeB = jetB.GetCombatUnitType();

        float multiplierAtoB = UnitAffinityManager.Instance.GetMultiplier(typeA, typeB);
        float multiplierBtoA = UnitAffinityManager.Instance.GetMultiplier(typeB, typeA);

        float baseDamageJet = PrototypeGameManager.Instance?.baseDamageJet ?? 50f;
        float baseDamageGround = PrototypeGameManager.Instance?.baseDamageGround ?? 40f;

        // ✅ 랜덤 ±10% 반영
        float damageAtoB = baseDamageJet * multiplierAtoB * Random.Range(0.9f, 1.1f);
        float damageBtoA = baseDamageJet * multiplierBtoA * Random.Range(0.9f, 1.1f);

        jetA.currentHP -= damageBtoA;
        jetB.currentHP -= damageAtoB;

        bool aDestroyed = jetA.currentHP <= 0f;
        bool bDestroyed = jetB.currentHP <= 0f;

        string labelA = ColoredName(jetA.title, jetA.teamType == JetStatus.TeamType.Blue ? "blue" : "red");
        string labelB = ColoredName(jetB.title, jetB.teamType == JetStatus.TeamType.Blue ? "blue" : "red");

        List<string> log = new()
        {
            $"{labelA} dealt {damageAtoB:F0} to {labelB} (HP: {Mathf.Max(0, Mathf.FloorToInt(jetB.currentHP))})",
            $"{labelB} dealt {damageBtoA:F0} to {labelA} (HP: {Mathf.Max(0, Mathf.FloorToInt(jetA.currentHP))})"
        };
        if (aDestroyed) log.Add($"{labelA} has been destroyed");
        if (bDestroyed) log.Add($"{labelB} has been destroyed");

        resultPopup.gameObject.SetActive(true);
        resultPopup.ShowResult(log, () =>
        {
            PrototypeGameManager.Instance?.ResumeGameplay();

            if (aDestroyed)
                Destroy(jetA.gameObject);
            else
                jetA.GetComponent<JetMover>()?.BeginReturn();

            if (bDestroyed)
                Destroy(jetB.gameObject);
            else
                jetB.GetComponent<JetMover>()?.BeginReturn();
        });
    }

    private string ColoredName(string name, string team)
    {
        return team == "blue" ? $"<color=#5080FF>{name}</color>"
             : team == "red" ? $"<color=#FF5050>{name}</color>"
             : name;
    }
}
