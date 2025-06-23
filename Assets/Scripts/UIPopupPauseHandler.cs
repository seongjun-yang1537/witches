using UnityEngine;

public class UIPopupPauseHandler : MonoBehaviour
{
    private bool pausedByThis = false;

    void OnEnable()
    {
        if (PrototypeGameManager.Instance != null)
        {
            PrototypeGameManager.Instance.PauseGameplay();
            pausedByThis = true;
            Debug.Log("[UIPopupPauseHandler] 게임 일시정지");
        }
        else
        {
            Debug.LogWarning("[UIPopupPauseHandler] Pause 실패 - PrototypeGameManager.Instance is null");
        }
    }

    void OnDisable()
    {
        if (PrototypeGameManager.Instance != null && pausedByThis)
        {
            PrototypeGameManager.Instance.ResumeGameplay();
            Debug.Log("[UIPopupPauseHandler] 게임 재개");
        }
        else
        {
            Debug.Log("[UIPopupPauseHandler] 비활성화 되었지만 Resume 하지 않음");
        }
    }
}
