// BorderLineDrawer.cs
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BorderLineDrawer : MonoBehaviour
{
    [Header("City Settings")]
    [Tooltip("CityStatus 컴포넌트를 가진 도시 오브젝트의 태그")]
    public string cityTag = "City";

    [Header("Border Mode")]
    [Tooltip("true면 카메라 Viewport 기준으로 선을 그림")]
    public bool useCamera = true;

    private LineRenderer lineRenderer;
    private Camera cam;

    void Awake()
    {
        // MainCamera 캐싱
        cam = Camera.main;
        if (cam == null)
            Debug.LogError("[BorderLineDrawer] MainCamera 태그가 설정된 카메라가 없습니다.");

        // LineRenderer 초기화
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        // 너비와 색상은 Inspector 설정을 그대로 사용합니다.
    }

    void LateUpdate()
    {
        UpdateBorderLine();
    }

    /// <summary>
    /// CityStatus.owner에 따라 Red/Blue Z 범위를 계산하고,
    /// 그 중간값 Z 평면 위에 카메라 뷰 기준 가로선을 그립니다.
    /// </summary>
    public void UpdateBorderLine()
    {
        // 1) CityStatus 컴포넌트 수집
        var cities = GameObject.FindGameObjectsWithTag(cityTag)
            .Select(go => go.GetComponent<CityStatus>())
            .Where(cs => cs != null)
            .ToList();
        if (cities.Count == 0)
            return;

        // 2) Red(위쪽) / Blue(아래쪽) Z 값 분리
        var redZs = cities.Where(c => c.owner == ArmyStatus.TeamType.Red)
                           .Select(c => c.transform.position.z);
        var blueZs = cities.Where(c => c.owner == ArmyStatus.TeamType.Blue)
                           .Select(c => c.transform.position.z);
        if (!redZs.Any() || !blueZs.Any())
            return;

        // 3) borderZ(가운데 Z)와 avgY 계산
        float minRedZ = redZs.Min();
        float maxBlueZ = blueZs.Max();
        float borderZ = (minRedZ + maxBlueZ) * 0.5f;
        float avgY = cities.Select(c => c.transform.position.y).Average();

        Vector3 leftPos, rightPos;

        if (useCamera && cam != null)
        {
            // Viewport 좌우 중간(y=0.5) 지점에서 레이 쏴 X 좌표 계산
            Ray rayL = cam.ViewportPointToRay(new Vector3(0f, 0.5f, 0f));
            Ray rayR = cam.ViewportPointToRay(new Vector3(1f, 0.5f, 0f));

            float tL = (borderZ - rayL.origin.z) / rayL.direction.z;
            float tR = (borderZ - rayR.origin.z) / rayR.direction.z;

            float xL = rayL.origin.x + rayL.direction.x * tL;
            float xR = rayR.origin.x + rayR.direction.x * tR;

            leftPos = new Vector3(xL, avgY, borderZ);
            rightPos = new Vector3(xR, avgY, borderZ);
        }
        else
        {
            // 카메라 모드가 아닐 때 간단한 고정 X 범위 (필요시 leftPoint/rightPoint 로 확장)
            leftPos = new Vector3(-10f, avgY, borderZ);
            rightPos = new Vector3(10f, avgY, borderZ);
        }

        // 4) LineRenderer에 위치 반영
        lineRenderer.SetPosition(0, leftPos);
        lineRenderer.SetPosition(1, rightPos);

        // (선 시각화용 디버그 라인, 필요 없으면 주석 처리)
        Debug.DrawLine(leftPos, rightPos, Color.green, 0.1f);
    }

    void OnValidate()
    {
        // 에디터에서도 바로 갱신
        if (Application.isPlaying && lineRenderer != null)
            UpdateBorderLine();
    }
}
