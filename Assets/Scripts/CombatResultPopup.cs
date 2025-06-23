using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 전투 결과 로그를 한 줄씩 출력하고,
/// 닫기 버튼 클릭 시 후속 처리를 트리거하는 팝업 UI
/// </summary>
public class CombatResultPopup : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI logText;
    public Button closeButton;

    [Header("Settings")]
    [Tooltip("Delay (seconds) between each line of text")]
    public float lineDelay = 0.5f;

    private Coroutine currentRoutine = null;
    private System.Action onCloseCallback;

    void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
            closeButton.gameObject.SetActive(false);
        }

        if (logText != null)
            logText.text = "";

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 전투 로그 출력 시작 + UI 최상단으로 이동
    /// </summary>
    /// <param name="lines">로그 문자열 목록</param>
    /// <param name="onClose">닫기 후 호출될 콜백</param>
    public void ShowResult(List<string> lines, System.Action onClose)
    {
        onCloseCallback = onClose;

        // ✅ 팝업 활성화
        gameObject.SetActive(true);

        // ✅ UI 계층에서 최상단으로 이동
        transform.SetAsLastSibling();

        logText.text = "";
        closeButton.gameObject.SetActive(false);

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayLogLines(lines));
    }

    IEnumerator PlayLogLines(List<string> lines)
    {
        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                logText.text += line + "\n";
                yield return new WaitForSeconds(lineDelay);
            }
        }

        closeButton.gameObject.SetActive(true);
    }

    void OnCloseClicked()
    {
        gameObject.SetActive(false);

        onCloseCallback?.Invoke();
        onCloseCallback = null;
    }
}
