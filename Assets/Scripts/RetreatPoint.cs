using UnityEngine;

[RequireComponent(typeof(Transform))]
public class RetreatPoint : MonoBehaviour
{
    // ArmyStatus.TeamType 과 동일한 enum 사용
    public ArmyStatus.TeamType teamType = ArmyStatus.TeamType.Blue;

    // (옵션) Gizmo로 씬 상 표시
    void OnDrawGizmos()
    {
        Gizmos.color = (teamType == ArmyStatus.TeamType.Blue) ? Color.blue : Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
