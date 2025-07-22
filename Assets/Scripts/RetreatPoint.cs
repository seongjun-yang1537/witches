using UnityEngine;

[RequireComponent(typeof(Transform))]
public class RetreatPoint : MonoBehaviour
{
    // ArmyStatus.TeamType �� ������ enum ���
    public ArmyStatus.TeamType teamType = ArmyStatus.TeamType.Blue;

    // (�ɼ�) Gizmo�� �� �� ǥ��
    void OnDrawGizmos()
    {
        Gizmos.color = (teamType == ArmyStatus.TeamType.Blue) ? Color.blue : Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
