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
            Debug.Log("[UIPopupPauseHandler] ���� �Ͻ�����");
        }
        else
        {
            Debug.LogWarning("[UIPopupPauseHandler] Pause ���� - PrototypeGameManager.Instance is null");
        }
    }

    void OnDisable()
    {
        if (PrototypeGameManager.Instance != null && pausedByThis)
        {
            PrototypeGameManager.Instance.ResumeGameplay();
            Debug.Log("[UIPopupPauseHandler] ���� �簳");
        }
        else
        {
            Debug.Log("[UIPopupPauseHandler] ��Ȱ��ȭ �Ǿ����� Resume ���� ����");
        }
    }
}
