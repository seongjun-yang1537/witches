using UnityEngine;

public class PrototypeGameManager : MonoBehaviour
{
    public static PrototypeGameManager Instance { get; private set; }

    public bool IsGameplayPaused { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void PauseGameplay()
    {
        IsGameplayPaused = true;
        Debug.Log("[Prototype] 게임 로직 정지됨");
    }

    public void ResumeGameplay()
    {
        IsGameplayPaused = false;
        Debug.Log("[Prototype] 게임 로직 재개됨");
    }

    public void TogglePause()
    {
        if (IsGameplayPaused)
            ResumeGameplay();
        else
            PauseGameplay();
    }
}
