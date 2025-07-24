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

    // 현재 프레임 기준 Z 국경선 위치 (모든 스크립트에서 접근 가능)
    public static float currentBorderZ = 0f;

    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
            Debug.LogError("[BorderLineDrawer] MainCamera 태그가 설정된 카메라가 없습니다.");

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }

    void LateUpdate()
    {
        UpdateBorderLine();
    }

    public void UpdateBorderLine()
    {
        // 1) CityStatus 수집
        var cities = GameObject.FindGameObjectsWithTag(cityTag)
            .Select(go => go.GetComponent<CityStatus>())
            .Where(cs => cs != null).ToList();
        if (cities.Count == 0) return;

        // 2) Red/Blue Z 분리
        var redZs = cities.Where(c => c.owner == ArmyStatus.TeamType.Red)
                           .Select(c => c.transform.position.z);
        var blueZs = cities.Where(c => c.owner == ArmyStatus.TeamType.Blue)
                           .Select(c => c.transform.position.z);
        if (!redZs.Any() || !blueZs.Any()) return;

        // 3) 국경 Z 및 Y 높이 계산
        float minRedZ = redZs.Min();
        float maxBlueZ = blueZs.Max();
        float borderZ = (minRedZ + maxBlueZ) * 0.5f;
        float avgY = cities.Select(c => c.transform.position.y).Average();
        currentBorderZ = borderZ;

        Vector3 leftPos, rightPos;
        if (useCamera && cam != null)
        {
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
            leftPos = new Vector3(-10f, avgY, borderZ);
            rightPos = new Vector3(10f, avgY, borderZ);
        }

        // 4) 그리기
        lineRenderer.SetPosition(0, leftPos);
        lineRenderer.SetPosition(1, rightPos);
        Debug.DrawLine(leftPos, rightPos, Color.green, 0.1f);
    }

    void OnValidate()
    {
        if (Application.isPlaying && lineRenderer != null)
            UpdateBorderLine();
    }
}
