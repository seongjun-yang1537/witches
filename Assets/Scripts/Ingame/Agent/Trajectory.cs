using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    [Serializable]
    public class Trajectory
    {
        private const int SAMPLES = 30;

        [SerializeField]
        public List<Vector3> samples { get; private set; }
        public int Count => samples?.Count ?? 0;
        public bool IsEmpty { get => Count == 0; }

        public float Length { get => GetLength(); }

        private Trajectory()
        {
            samples = new List<Vector3>();
        }

        public Vector3 Interpolation(float ratio)
        {
            ratio *= SAMPLES;
            int startIdx = Mathf.FloorToInt(ratio);
            if (startIdx == samples.Count - 1) return samples[startIdx];
            int nextIdx = startIdx + 1;
            return Vector3.Lerp(samples[startIdx], samples[nextIdx], ratio - startIdx);
        }

        public static Trajectory CreateLine(
            PlaneSpec planeSpec,
            Vector3 startPosition,
            Vector3 startTangent,
            Vector3 endPosition
        )
        {
            Vector3 midPositon = startPosition + startTangent * planeSpec.minLength;

            Trajectory trajectory = new();

            for (int i = 0; i <= SAMPLES; i++)
            {
                float t = (float)i / SAMPLES;
                float oneMinusT = 1f - t;

                Vector3 pointOnCurve =
                    oneMinusT * oneMinusT * startPosition +
                    2f * oneMinusT * t * midPositon +
                    t * t * endPosition;

                trajectory.samples.Add(pointOnCurve);
            }

            float length = trajectory.Length;
            if (length < planeSpec.minLength ||
                planeSpec.maxLength < length ||
                !CheckSampleAngles(planeSpec, trajectory.samples)
            )
                trajectory.Clear();

            return trajectory;
        }

        public void Clear()
        {
            samples.Clear();
        }

        private static bool CheckSampleAngles(PlaneSpec planeSpec, List<Vector3> samples)
        {
            List<float> angles = new();

            for (int i = 1; i < samples.Count - 1; i++)
            {
                Vector3 u = samples[i - 1];
                Vector3 v = samples[i];
                Vector3 w = samples[i + 1];

                Vector3 vu = (v - u).normalized;
                Vector3 wv = (w - v).normalized;

                angles.Add(Vector3.Angle(vu, wv));
            }

            for (int i = 0; i < angles.Count; i++)
                if (angles[i] > planeSpec.maxDegreePer)
                    return false;

            return true;
        }

        public float GetLength()
        {
            float length = 0f;
            if (Count < 2) return length;

            for (int i = 1; i <= SAMPLES; i++)
                length += Vector3.Distance(samples[i - 1], samples[i]);

            return length;
        }
    }
}