// BorderLineDrawer.cs
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BorderLineDrawer : MonoBehaviour
{
    [Header("City Settings")]
    [Tooltip("CityStatus ������Ʈ�� ���� ���� ������Ʈ�� �±�")]
    public string cityTag = "City";

    [Header("Border Mode")]
    [Tooltip("true�� ī�޶� Viewport �������� ���� �׸�")]
    public bool useCamera = true;

    private LineRenderer lineRenderer;
    private Camera cam;

    void Awake()
    {
        // MainCamera ĳ��
        cam = Camera.main;
        if (cam == null)
            Debug.LogError("[BorderLineDrawer] MainCamera �±װ� ������ ī�޶� �����ϴ�.");

        // LineRenderer �ʱ�ȭ
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        // �ʺ�� ������ Inspector ������ �״�� ����մϴ�.
    }

    void LateUpdate()
    {
        UpdateBorderLine();
    }

    /// <summary>
    /// CityStatus.owner�� ���� Red/Blue Z ������ ����ϰ�,
    /// �� �߰��� Z ��� ���� ī�޶� �� ���� ���μ��� �׸��ϴ�.
    /// </summary>
    public void UpdateBorderLine()
    {
        // 1) CityStatus ������Ʈ ����
        var cities = GameObject.FindGameObjectsWithTag(cityTag)
            .Select(go => go.GetComponent<CityStatus>())
            .Where(cs => cs != null)
            .ToList();
        if (cities.Count == 0)
            return;

        // 2) Red(����) / Blue(�Ʒ���) Z �� �и�
        var redZs = cities.Where(c => c.owner == ArmyStatus.TeamType.Red)
                           .Select(c => c.transform.position.z);
        var blueZs = cities.Where(c => c.owner == ArmyStatus.TeamType.Blue)
                           .Select(c => c.transform.position.z);
        if (!redZs.Any() || !blueZs.Any())
            return;

        // 3) borderZ(��� Z)�� avgY ���
        float minRedZ = redZs.Min();
        float maxBlueZ = blueZs.Max();
        float borderZ = (minRedZ + maxBlueZ) * 0.5f;
        float avgY = cities.Select(c => c.transform.position.y).Average();

        Vector3 leftPos, rightPos;

        if (useCamera && cam != null)
        {
            // Viewport �¿� �߰�(y=0.5) �������� ���� �� X ��ǥ ���
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
            // ī�޶� ��尡 �ƴ� �� ������ ���� X ���� (�ʿ�� leftPoint/rightPoint �� Ȯ��)
            leftPos = new Vector3(-10f, avgY, borderZ);
            rightPos = new Vector3(10f, avgY, borderZ);
        }

        // 4) LineRenderer�� ��ġ �ݿ�
        lineRenderer.SetPosition(0, leftPos);
        lineRenderer.SetPosition(1, rightPos);

        // (�� �ð�ȭ�� ����� ����, �ʿ� ������ �ּ� ó��)
        Debug.DrawLine(leftPos, rightPos, Color.green, 0.1f);
    }

    void OnValidate()
    {
        // �����Ϳ����� �ٷ� ����
        if (Application.isPlaying && lineRenderer != null)
            UpdateBorderLine();
    }
}
