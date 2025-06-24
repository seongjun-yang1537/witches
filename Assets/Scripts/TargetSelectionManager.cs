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
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 mouseWorld = ray.GetPoint(enter);
            float radius = 1.0f;

            Collider[] hits = Physics.OverlapSphere(mouseWorld, radius, targetLayer);

            Transform selectedTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                // ✅ JetStatus 우선 체크
                JetStatus jet = hit.GetComponent<JetStatus>();
                if (jet != null && jet.teamType == JetStatus.TeamType.Red && !jet.isHealing)
                {
                    float dist = Vector3.Distance(mouseWorld, jet.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        selectedTarget = jet.transform;
                    }
                    continue; // 우선적으로 처리
                }

                // ✅ ArmyStatus 체크
                ArmyStatus army = hit.GetComponent<ArmyStatus>();
                if (army != null && army.teamType == ArmyStatus.TeamType.Red)
                {
                    float dist = Vector3.Distance(mouseWorld, army.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        selectedTarget = army.transform;
                    }
                }
            }

            if (selectedTarget != null)
            {
                currentJet.SetTarget(selectedTarget);
                currentJet = null;
                lineRenderer.enabled = false;
                messageUI.HideMessage();

                PrototypeGameManager.Instance?.ResumeGameplay();
            }
            else
            {
                Debug.LogWarning("[TargetSelection] 적 타겟을 찾지 못함");
            }
        }
    }




    void CancelDeployment()
    {
        Debug.Log("[TargetSelection] 출격 취소");

        if (currentJet != null)
        {
            currentJet.originItemUI?.SetAvailable(true);

            // ✅ Destroy 대신 비활성화
            currentJet.gameObject.SetActive(false);
            currentJet = null;
        }

        lineRenderer.enabled = false;
        messageUI.HideMessage();

        if (airbasePopup != null)
        {
            airbasePopup.SetActive(true);
        }
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
