using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarlogManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static WarlogManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Scroll View의 Content Transform")]
    [SerializeField] private RectTransform content;
    [Tooltip("로그 항목으로 사용할 Entry 프리팹")]
    [SerializeField] private GameObject entryPrefab;
    [Tooltip("Scroll Rect 컴포넌트 (자동 스크롤용)")]
    [SerializeField] private ScrollRect scrollRect;

    // 내부 이벤트 기록용 리스트 (필요 시 활용)
    private readonly List<WarlogEvent> events = new List<WarlogEvent>();

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[Warlog] Instance set on " + gameObject.name);
    }

    /// <summary>
    /// 새로운 Warlog 이벤트를 화면에 표시합니다.
    /// 최신 이벤트가 위로 가도록 Content 맨 위에 삽입합니다.
    /// </summary>
    /// <param name="unitName">유닛 이름</param>
    /// <param name="statusText">상태 텍스트</param>
    public void LogEvent(string unitName, string statusText)
    {
        Debug.Log($"[Warlog] LogEvent called: {unitName} / {statusText}");
        if (entryPrefab == null || content == null)
        {
            Debug.LogError("[Warlog] entryPrefab or content is null!");
            return;
        }

        // 시간 기록 (게임 시작 이후 경과 초)
        float t = Time.time;
        // 내부 리스트에 보관 (필요시 과거 보기 등에 활용)
        events.Insert(0, new WarlogEvent(t, unitName, statusText));

        // UI 생성
        GameObject go = Instantiate(entryPrefab, content);
        // 최신 항목이 맨 위로
        go.transform.SetAsFirstSibling();

        // 텍스트 세팅
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = FormatEntry(t, unitName, statusText);
        }

        // 레이아웃 및 스크롤 위치 업데이트
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// Time.time 값을 시:분:초 형식으로 바꿔서 반환합니다.
    /// </summary>
    private string FormatEntry(float timeSeconds, string unitName, string statusText)
    {
        TimeSpan ts = TimeSpan.FromSeconds(timeSeconds);
        string timestamp = $"[{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}]";
        return $"{timestamp} {unitName}: {statusText}";
    }

    // (선택) 내부 기록을 외부에서 조회할 수 있는 API
    public IReadOnlyList<WarlogEvent> GetAllEvents() => events.AsReadOnly();
}

/// <summary>
/// Warlog 이벤트 데이터 모델
/// </summary>
public struct WarlogEvent
{
    public float time;
    public string unitName;
    public string statusText;

    public WarlogEvent(float time, string unitName, string statusText)
    {
        this.time = time;
        this.unitName = unitName;
        this.statusText = statusText;
    }
}
