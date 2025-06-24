using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameObject defeatPanel;   // Canvas 아래에 배치된 UI
    public GameObject victoryPanel;

    private List<ArmyStatus> blueArmies = new();
    private List<ArmyStatus> redArmies = new();
    private List<JetStatus> redJets = new();

    private bool gameEnded = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 처음에는 비활성화
        defeatPanel?.SetActive(false);
        victoryPanel?.SetActive(false);
    }

    // ✅ 유닛 등록
    public void RegisterArmy(ArmyStatus army)
    {
        if (army.teamType == ArmyStatus.TeamType.Blue)
            blueArmies.Add(army);
        else
            redArmies.Add(army);
    }

    public void RegisterJet(JetStatus jet)
    {
        if (jet.teamType == JetStatus.TeamType.Red)
            redJets.Add(jet);
    }

    // ✅ 유닛 파괴 시 해제 + 조건 검사
    public void UnregisterArmy(ArmyStatus army)
    {
        if (army.teamType == ArmyStatus.TeamType.Blue)
            blueArmies.Remove(army);
        else
            redArmies.Remove(army);

        CheckGameEnd();
    }

    public void UnregisterJet(JetStatus jet)
    {
        if (jet.teamType == JetStatus.TeamType.Red)
            redJets.Remove(jet);

        CheckGameEnd();
    }

    void CheckGameEnd()
    {
        if (gameEnded) return;

        if (blueArmies.Count == 0)
        {
            gameEnded = true;
            ShowGameOver();
        }

        if (redArmies.Count == 0 && redJets.Count == 0)
        {
            gameEnded = true;
            ShowEnding();
        }
    }

    void ShowGameOver()
    {
        PrototypeGameManager.Instance?.PauseGameplay();

        if (defeatPanel == null)
        {
            Debug.LogWarning("[GameStateManager] DefeatPanel is missing (maybe destroyed already)");
            return;
        }

        if (defeatPanel.gameObject != null)
        {
            defeatPanel.SetActive(true);
            defeatPanel.transform.SetAsLastSibling();
        }
    }

    void ShowEnding()
    {
        PrototypeGameManager.Instance?.PauseGameplay();
        victoryPanel?.SetActive(true);
        victoryPanel.transform.SetAsLastSibling();
    }
}
