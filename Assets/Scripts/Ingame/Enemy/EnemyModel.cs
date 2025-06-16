using UnityEngine;

namespace Ingame
{

    public class EnemyModel : AgentModel
    {
        // private void Awake()
        // {
        //     int seed = (int)(DateTime.Now.Ticks & 0xFFFFFFFF);
        //     rng = MT19937.Create(gameObject.GetInstanceID() + seed);
        // }

        // public override void OnMovePhase()
        // {
        //     base.OnMovePhase();
        //     Vector3 randomNormal = new Vector3(
        //         rng.NextFloat(-1.0f, 1.0f),
        //         0f,
        //         rng.NextFloat(-1.0f, 1.0f)
        //     ).normalized;
        //     float randomDelta = rng.NextFloat(3.0f, 5.0f);

        //     SetTargetPosition(randomNormal * randomDelta);
        // }
    }
}