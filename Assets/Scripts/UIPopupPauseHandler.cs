using UnityEngine;

public class UIPopupPauseHandler : MonoBehaviour
{
    void OnEnable()
    {
        if (PrototypeGameManager.Instance != null)
        {
            PrototypeGameManager.Instance.PauseGameplay();
            Debug.Log("[UIPopupPauseHandler] ���� �Ͻ�����");
        }
        else
        {
            Debug.LogWarning("[UIPopupPauseHandler] PrototypeGameManager.Instance is null. Pause ����");
        }
    }

    void OnDisable()
    {
        if (PrototypeGameManager.Instance != null)
        {
            PrototypeGameManager.Instance.ResumeGameplay();
            Debug.Log("[UIPopupPauseHandler] ���� �簳");
        }
    }
}
