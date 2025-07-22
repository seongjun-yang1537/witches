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

        // ���� �ִ� ��� RetreatPoint ������Ʈ �˻�
        var all = FindObjectsOfType<RetreatPoint>();
        foreach (var rp in all)
        {
            if (rp.teamType == ArmyStatus.TeamType.Blue)
                bluePoints.Add(rp.transform);
            else
                redPoints.Add(rp.transform);
        }
    }

    /// <summary>�־��� ���� ���� ����� ���� ���� ��ȯ</summary>
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
