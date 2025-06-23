using UnityEngine;
using TMPro;

public class TargetMessageUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;

    void Awake()
    {
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    public void ShowMessage(string msg)
    {
        if (messageText == null) return;

        messageText.text = msg;
        messageText.gameObject.SetActive(true);
    }

    public void HideMessage()
    {
        if (messageText == null) return;

        messageText.gameObject.SetActive(false);
    }
}
