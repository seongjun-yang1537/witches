using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarlogManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static WarlogManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Scroll View�� Content Transform")]
    [SerializeField] private RectTransform content;
    [Tooltip("�α� �׸����� ����� Entry ������")]
    [SerializeField] private GameObject entryPrefab;
    [Tooltip("Scroll Rect ������Ʈ (�ڵ� ��ũ�ѿ�)")]
    [SerializeField] private ScrollRect scrollRect;

    // ���� �̺�Ʈ ��Ͽ� ����Ʈ (�ʿ� �� Ȱ��)
    private readonly List<WarlogEvent> events = new List<WarlogEvent>();

    private void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[Warlog] Instance set on " + gameObject.name);
    }

    /// <summary>
    /// ���ο� Warlog �̺�Ʈ�� ȭ�鿡 ǥ���մϴ�.
    /// �ֽ� �̺�Ʈ�� ���� ������ Content �� ���� �����մϴ�.
    /// </summary>
    /// <param name="unitName">���� �̸�</param>
    /// <param name="statusText">���� �ؽ�Ʈ</param>
    public void LogEvent(string unitName, string statusText)
    {
        Debug.Log($"[Warlog] LogEvent called: {unitName} / {statusText}");
        if (entryPrefab == null || content == null)
        {
            Debug.LogError("[Warlog] entryPrefab or content is null!");
            return;
        }

        // �ð� ��� (���� ���� ���� ��� ��)
        float t = Time.time;
        // ���� ����Ʈ�� ���� (�ʿ�� ���� ���� � Ȱ��)
        events.Insert(0, new WarlogEvent(t, unitName, statusText));

        // UI ����
        GameObject go = Instantiate(entryPrefab, content);
        // �ֽ� �׸��� �� ����
        go.transform.SetAsFirstSibling();

        // �ؽ�Ʈ ����
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = FormatEntry(t, unitName, statusText);
        }

        // ���̾ƿ� �� ��ũ�� ��ġ ������Ʈ
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// Time.time ���� ��:��:�� �������� �ٲ㼭 ��ȯ�մϴ�.
    /// </summary>
    private string FormatEntry(float timeSeconds, string unitName, string statusText)
    {
        TimeSpan ts = TimeSpan.FromSeconds(timeSeconds);
        string timestamp = $"[{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}]";
        return $"{timestamp} {unitName}: {statusText}";
    }

    // (����) ���� ����� �ܺο��� ��ȸ�� �� �ִ� API
    public IReadOnlyList<WarlogEvent> GetAllEvents() => events.AsReadOnly();
}

/// <summary>
/// Warlog �̺�Ʈ ������ ��
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
