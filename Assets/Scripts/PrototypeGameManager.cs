using UnityEngine;

public class PrototypeGameManager : MonoBehaviour
{
    [Header("Combat Settings")]
    public float armyCombatTickInterval = 0.5f;
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
        Debug.Log("[Prototype] ���� ���� ������");
    }

    public void ResumeGameplay()
    {
        IsGameplayPaused = false;
        Debug.Log("[Prototype] ���� ���� �簳��");
    }

    public void TogglePause()
    {
        if (IsGameplayPaused)
            ResumeGameplay();
        else
            PauseGameplay();
    }
}
