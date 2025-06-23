using UnityEngine;

public class UnitAffinityManager : MonoBehaviour
{
    public static UnitAffinityManager Instance;

    public UnitAffinityTable affinityTable;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public float GetMultiplier(CombatUnitType attacker, CombatUnitType defender)
    {
        if (affinityTable != null)
            return affinityTable.GetMultiplier(attacker, defender);
        else
            return 1.0f;
    }
}
