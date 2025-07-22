using System.Collections.Generic;
using UnityEngine;

public class RallyPointManager : MonoBehaviour
{
    public static RallyPointManager Instance { get; private set; }

    private List<Transform> redPoints = new List<Transform>();
    private List<Transform> bluePoints = new List<Transform>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 씬에 있는 모든 RetreatPoint 컴포넌트 검색
        var all = FindObjectsOfType<RetreatPoint>();
        foreach (var rp in all)
        {
            if (rp.teamType == ArmyStatus.TeamType.Blue)
                bluePoints.Add(rp.transform);
            else
                redPoints.Add(rp.transform);
        }
    }

    /// <summary>주어진 팀의 가장 가까운 후퇴 지점 반환</summary>
    public Transform GetNearestRetreatPoint(ArmyStatus.TeamType team, Vector3 fromPos)
    {
        List<Transform> list = (team == ArmyStatus.TeamType.Blue) ? bluePoints : redPoints;
        Transform best = null;
        float minDist = float.MaxValue;
        foreach (var t in list)
        {
            float d = Vector3.SqrMagnitude(t.position - fromPos);
            if (d < minDist)
            {
                minDist = d;
                best = t;
            }
        }
        return best;
    }
}
