using UnityEngine;

[CreateAssetMenu(fileName = "AirbaseItemData", menuName = "ScriptableObjects/AirbaseItemData", order = 0)]
public class AirbaseItemData : ScriptableObject
{
    [Header("��ü �̸�")]
    public string unitName;

    [Header("������ �̹��� (UI��)")]
    public Sprite jetSprite;

    [Header("���Ϸ� �̹��� (UI��)")]
    public Sprite pilotSprite;

    [Header("���� �ؽ�Ʈ")]
    [TextArea]
    public string description;

    [Header("��� �� ������ ������ ������")]
    public GameObject unitPrefab;
}
