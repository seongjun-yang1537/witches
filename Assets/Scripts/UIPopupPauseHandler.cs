using UnityEngine;

public class UIPopupPauseHandler : MonoBehaviour
{
    void OnEnable()
    {
        if (PrototypeGameManager.Instance != null)
        {
            PrototypeGameManager.Instance.PauseGameplay();
            Debug.Log("[UIPopupPauseHandler] 게임 일시정지");
        }
        else
        {
            Debug.LogWarning("[UIPopupPauseHandler] PrototypeGameManager.Instance is null. Pause 실패");
        }
    }

    void OnDisable()
    {
        if (PrototypeGameManager.Instance != null)
        {
            PrototypeGameManager.Instance.ResumeGameplay();
            Debug.Log("[UIPopupPauseHandler] 게임 재개");
        }
    }
}
