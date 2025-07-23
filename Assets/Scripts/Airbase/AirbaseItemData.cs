using UnityEngine;

[CreateAssetMenu(fileName = "AirbaseItemData", menuName = "ScriptableObjects/AirbaseItemData", order = 0)]
public class AirbaseItemData : ScriptableObject
{
    [Header("기체 이름")]
    public string unitName;

    [Header("타입")]
    public JetStatus.JetType jetType;

    [Header("전투기 이미지 (UI용)")]
    public Sprite jetSprite;

    [Header("파일럿 이미지 (UI용)")]
    public Sprite pilotSprite;

    [Header("설명 텍스트")]
    [TextArea]
    public string description;

    [Header("출격 시 생성할 전투기 프리팹")]
    public GameObject unitPrefab;
}
