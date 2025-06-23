using UnityEngine;
using UnityEngine.EventSystems;

public class TargetSelectionManager : MonoBehaviour
{
    public static TargetSelectionManager Instance { get; private set; }

    private JetMover currentJet;
    private LineRenderer lineRenderer;

    [Header("UI Manager")]
    public TargetMessageUI messageUI;  // "Select a Target" 메시지 제어용

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

        // 라인 렌더러 구성
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (currentJet == null) return;

        // 마우스 위치 추적 + 시각화
        Vector3 cursorWorld = GetMouseWorldPoint();
        lineRenderer.SetPosition(0, currentJet.transform.position);
        lineRenderer.SetPosition(1, cursorWorld);
        lineRenderer.enabled = true;

        // 좌클릭 = 타겟 지정
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            TrySelectTarget(cursorWorld);
        }

        // 우클릭 또는 ESC = 출격 취소
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
            // 기본 fallback
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
                // 타겟 지정 성공
                Debug.Log($"[TargetSelection] 타겟 지정: {targetStatus.name}");

                // JetMover에게 타겟 전달 (임시 - 추후 JetStatus 기반 이동 구현 필요)
                currentJet.SetTarget(hit.collider.transform);

                currentJet = null;
                lineRenderer.enabled = false;
                messageUI.HideMessage();
            }
        }
    }

    void CancelDeployment()
    {
        Debug.Log("[TargetSelection] 출격 취소");

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
