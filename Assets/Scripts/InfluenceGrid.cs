using UnityEngine;

public class InfluenceGrid : MonoBehaviour
{
    [Header("Grid ����")]
    [Tooltip("���� �� ����")]
    public int cols = 16;
    [Tooltip("���� �� ����")]
    public int rows = 12;
    [Tooltip("�� �� ���� ���� (����)")]
    public float cellSize = 1f;
    [Header("�׸��� ���� (�������� Plane)")]
    public Transform gridOrigin;  // Plane ������Ʈ�� Transform

    [Header("���� ����� ����")]
    [Tooltip("������ ��ġ�� ���� �ݰ� (����)")]
    public float influenceRadius = 3f;

    [Header("Retreat Point �����")]
    [Tooltip("�� RetreatPoint �� ������ ���� ����� ���")]
    public float retreatPointStrength = 1f;

    private float[,] inflBlue;
    private float[,] inflRed;

    void Awake()
    {
        inflBlue = new float[cols, rows];
        inflRed = new float[cols, rows];
    }

    /// <summary>
    /// �ֱ������� ȣ���Ͽ� ����� ���� �����մϴ�.
    /// </summary>
    public void UpdateMap()
    {
        // 1) �ʱ�ȭ
        System.Array.Clear(inflBlue, 0, inflBlue.Length);
        System.Array.Clear(inflRed, 0, inflRed.Length);

        int radius = Mathf.CeilToInt(influenceRadius / cellSize);

        // 2) ���ֺ� ����� ����
        var units = FindObjectsOfType<ArmyStatus>();
        foreach (var unit in units)
        {
            Vector2Int gp = WorldToGrid(unit.transform.position);
            float strength = unit.currentHP / unit.maxHP;

            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = gp.x + dx;
                    int y = gp.y + dy;
                    if (x < 0 || x >= cols || y < 0 || y >= rows) continue;

                    float dist = new Vector2(dx, dy).magnitude * cellSize;
                    float weight = Mathf.Clamp01(1f - (dist / influenceRadius));
                    float add = strength * weight;

                    if (unit.teamType == ArmyStatus.TeamType.Blue)
                        inflBlue[x, y] += add;
                    else
                        inflRed[x, y] += add;
                }
        }

        // 3) RetreatPoint�� ����� ����
        var rps = FindObjectsOfType<RetreatPoint>();
        foreach (var rp in rps)
        {
            Vector2Int gpRp = WorldToGrid(rp.transform.position);
            float strengthRp = retreatPointStrength;

            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = gpRp.x + dx;
                    int y = gpRp.y + dy;
                    if (x < 0 || x >= cols || y < 0 || y >= rows) continue;

                    float dist = new Vector2(dx, dy).magnitude * cellSize;
                    float weight = Mathf.Clamp01(1f - (dist / influenceRadius));
                    float addRp = strengthRp * weight;

                    if (rp.teamType == ArmyStatus.TeamType.Blue)
                        inflBlue[x, y] += addRp;
                    else
                        inflRed[x, y] += addRp;
                }
        }

        // 4) ������ (�ɼ�)
        //Smooth(inflBlue);
        //Smooth(inflRed);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        // 1) Plane�� ���� ��ǥ�� ��ȯ
        Vector3 local = gridOrigin.InverseTransformPoint(worldPos);
        // Plane�� ���� x,z �� [-width/2, +width/2], [-height/2, +height/2]
        float halfW = cols * cellSize * 0.5f;
        float halfH = rows * cellSize * 0.5f;

        // 2) Plane ������ �׸��� �ε����� ��ȯ
        float fx = (local.x + halfW) / cellSize;
        float fy = (local.z + halfH) / cellSize;

        int ix = Mathf.FloorToInt(fx);
        int iy = Mathf.FloorToInt(fy);

        return new Vector2Int(ix, iy);
    }

    void Smooth(float[,] map)
    {
        var temp = (float[,])map.Clone();
        for (int x = 1; x < cols - 1; x++)
            for (int y = 1; y < rows - 1; y++)
            {
                float sum = 0f;
                for (int ox = -1; ox <= 1; ox++)
                    for (int oy = -1; oy <= 1; oy++)
                        sum += temp[x + ox, y + oy];
                map[x, y] = sum / 9f;
            }
    }

    /// <summary>
    /// Blue/Red ����� �迭 ��ȯ
    /// </summary>
    public float[,] GetBlueInfluence() => inflBlue;
    public float[,] GetRedInfluence() => inflRed;
}
