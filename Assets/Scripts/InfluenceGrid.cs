using UnityEngine;

public class InfluenceGrid : MonoBehaviour
{
    [Header("Grid 설정")]
    [Tooltip("가로 셀 개수")]
    public int cols = 16;
    [Tooltip("세로 셀 개수")]
    public int rows = 12;
    [Tooltip("셀 한 변의 길이 (유닛)")]
    public float cellSize = 1f;
    [Header("그리드 원점 (오버레이 Plane)")]
    public Transform gridOrigin;  // Plane 오브젝트의 Transform

    [Header("유닛 영향력 설정")]
    [Tooltip("유닛이 미치는 영향 반경 (유닛)")]
    public float influenceRadius = 3f;

    [Header("Retreat Point 영향력")]
    [Tooltip("각 RetreatPoint 에 적용할 고정 영향력 계수")]
    public float retreatPointStrength = 1f;

    private float[,] inflBlue;
    private float[,] inflRed;

    void Awake()
    {
        inflBlue = new float[cols, rows];
        inflRed = new float[cols, rows];
    }

    /// <summary>
    /// 주기적으로 호출하여 영향력 맵을 갱신합니다.
    /// </summary>
    public void UpdateMap()
    {
        // 1) 초기화
        System.Array.Clear(inflBlue, 0, inflBlue.Length);
        System.Array.Clear(inflRed, 0, inflRed.Length);

        int radius = Mathf.CeilToInt(influenceRadius / cellSize);

        // 2) 유닛별 영향력 투사
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

        // 3) RetreatPoint별 영향력 투사
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

        // 4) 스무딩 (옵션)
        //Smooth(inflBlue);
        //Smooth(inflRed);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        // 1) Plane의 로컬 좌표로 변환
        Vector3 local = gridOrigin.InverseTransformPoint(worldPos);
        // Plane의 로컬 x,z 는 [-width/2, +width/2], [-height/2, +height/2]
        float halfW = cols * cellSize * 0.5f;
        float halfH = rows * cellSize * 0.5f;

        // 2) Plane 로컬을 그리드 인덱스로 변환
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
    /// Blue/Red 영향력 배열 반환
    /// </summary>
    public float[,] GetBlueInfluence() => inflBlue;
    public float[,] GetRedInfluence() => inflRed;
}
