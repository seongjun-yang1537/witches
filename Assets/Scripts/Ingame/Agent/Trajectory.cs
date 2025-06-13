using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    [Serializable]
    public class Trajectory
    {
        [SerializeField]
        public List<Vector3> samples;

        public Vector3 Interpolate(float ratio)
        {
            if (samples == null || samples.Count < 2)
                return Vector3.zero;

            ratio = Mathf.Clamp01(ratio);

            float scaledPos = ratio * (samples.Count - 1);
            int index = Mathf.FloorToInt(scaledPos);
            int nextIndex = Mathf.Min(index + 1, samples.Count - 1);
            float t = scaledPos - index;

            return Vector3.Lerp(samples[index], samples[nextIndex], t);
        }
    }
}