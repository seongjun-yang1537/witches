using UnityEngine;
using UnityEngine.EventSystems;

public class TargetSelectionManager : MonoBehaviour
{
    public static TargetSelectionManager Instance { get; private set; }
    public GameObject airbasePopup;

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

    void TrySelectTarget(Vector3 _)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Y=0 평면과의 교차점 계산
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Y=0 평면
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 mouseWorld = ray.GetPoint(enter);

            Debug.Log($"[TargetSelection] mouseWorld = {mouseWorld}");

            float radius = 0.5f;
            Collider[] hits = Physics.OverlapSphere(mouseWorld, radius, targetLayer);

            if (hits.Length > 0)
            {
                var targetStatus = hits[0].GetComponent<ArmyStatus>();
                if (targetStatus != null)
                {
                    currentJet.SetTarget(hits[0].transform);
                    currentJet = null;
                    lineRenderer.enabled = false;
                    messageUI.HideMessage();

                    if (PrototypeGameManager.Instance != null)
                        PrototypeGameManager.Instance.ResumeGameplay();
                }
            }
            else
            {
                Debug.LogWarning($"[TargetSelection] 적을 찾지 못함 - mouseWorld: {mouseWorld}");
            }
        }
        else
        {
            Debug.LogWarning("[TargetSelection] 레이와 Y=0 평면이 교차하지 않음");
        }
    }


    void CancelDeployment()
    {
        Debug.Log("[TargetSelection] 출격 취소");

        if (currentJet != null)
        {
            // 1. 출격 취소된 전투기 제거
            currentJet.originItemUI?.SetAvailable(true); // ✅ 다시 활성화
            Destroy(currentJet.gameObject);
            currentJet = null;
        }

        // 2. 시각화 제거
        lineRenderer.enabled = false;

        // 3. 문구 숨기기
        messageUI.HideMessage();

        // 4. AirbasePopup 다시 열기
        if (airbasePopup != null)
        {
            airbasePopup.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[TargetSelection] AirbasePopup not found in scene!");
        }

        // 게임 재개
        if (PrototypeGameManager.Instance != null)
            PrototypeGameManager.Instance.ResumeGameplay();
    }


    public void BeginTargeting(JetMover jet)
    {
        currentJet = jet;

        // 게임 일시 정지
        if (PrototypeGameManager.Instance != null && !PrototypeGameManager.Instance.IsGameplayPaused)
        {
            PrototypeGameManager.Instance.PauseGameplay();
            Debug.Log("[TargetSelection] 게임 일시정지");
        }

        messageUI.ShowMessage("Select a Target");
    }
}
