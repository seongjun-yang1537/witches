using UnityEngine;

public class UIPopupCloser : MonoBehaviour
{
    public GameObject popupToClose;

    public void ClosePopup()
    {
        popupToClose.SetActive(false);
    }
}
