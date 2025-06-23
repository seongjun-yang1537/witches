using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AffinityPair
{
    public CombatUnitType attacker;
    public CombatUnitType defender;
    public float multiplier = 1.0f;
}

[CreateAssetMenu(fileName = "UnitAffinityTable", menuName = "Combat/Unit Affinity Table")]
public class UnitAffinityTable : ScriptableObject
{
    public List<AffinityPair> affinities = new List<AffinityPair>();

    public float GetMultiplier(CombatUnitType attacker, CombatUnitType defender)
    {
        foreach (var pair in affinities)
        {
            if (pair.attacker == attacker && pair.defender == defender)
                return pair.multiplier;
        }

        return 1.0f; // ±âº»°ª
    }
}
