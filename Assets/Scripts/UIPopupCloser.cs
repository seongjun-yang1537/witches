using UnityEngine;

public class UIPopupCloser : MonoBehaviour
{
    public GameObject popupToClose;

    public void ClosePopup()
    {
        if (popupToClose != null)
        {
            popupToClose.SetActive(false); // ✅ 실제 팝업을 닫는다
        }
        else
        {
            Debug.LogWarning("[UIPopupCloser] popupToClose 가 null입니다.");
        }
    }
}
