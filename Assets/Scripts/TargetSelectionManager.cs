using UnityEngine;
using UnityEngine.EventSystems;

public class TargetSelectionManager : MonoBehaviour
{
    public static TargetSelectionManager Instance { get; private set; }

    private JetMover currentJet;
    private LineRenderer lineRenderer;

    [Header("UI Manager")]
    public TargetMessageUI messageUI;  // "Select a Target" �޽��� �����

    [Header("Layer Mask")]
    public LayerMask targetLayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ���� ������ ����
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (currentJet == null) return;

        // ���콺 ��ġ ���� + �ð�ȭ
        Vector3 cursorWorld = GetMouseWorldPoint();
        lineRenderer.SetPosition(0, currentJet.transform.position);
        lineRenderer.SetPosition(1, cursorWorld);
        lineRenderer.enabled = true;

        // ��Ŭ�� = Ÿ�� ����
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            TrySelectTarget(cursorWorld);
        }

        // ��Ŭ�� �Ǵ� ESC = ��� ���
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelDeployment();
        }
    }

    Vector3 GetMouseWorldPoint()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
        {
            return hit.point;
        }
        else
        {
            // �⺻ fallback
            return ray.origin + ray.direction * 10f;
        }
    }

    void TrySelectTarget(Vector3 cursorWorld)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
        {
            var targetStatus = hit.collider.GetComponent<ArmyStatus>();
            if (targetStatus != null)
            {
                // Ÿ�� ���� ����
                Debug.Log($"[TargetSelection] Ÿ�� ����: {targetStatus.name}");

                // JetMover���� Ÿ�� ���� (�ӽ� - ���� JetStatus ��� �̵� ���� �ʿ�)
                currentJet.SetTarget(hit.collider.transform);

                currentJet = null;
                lineRenderer.enabled = false;
                messageUI.HideMessage();
            }
        }
    }

    void CancelDeployment()
    {
        Debug.Log("[TargetSelection] ��� ���");

        if (currentJet != null)
        {
            currentJet.originItemUI?.SetAvailable(true);
            Destroy(currentJet.gameObject);
            currentJet = null;
        }

        lineRenderer.enabled = false;
        messageUI.HideMessage();

        var airbasePopup = GameObject.Find("AirbasePopup");
        if (airbasePopup != null)
            airbasePopup.SetActive(true);
    }

    public void BeginTargeting(JetMover jet)
    {
        currentJet = jet;
        messageUI.ShowMessage("Select a Target");
    }
}
