using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Ingame
{
    [Serializable]
    public class Trajectory
    {
        [SerializeField]
        public List<Vector3> samples;
        public int Count { get => samples.Count; }

        public Trajectory(
            Vector3 handleDirection,
            float handleDistance,
            Vector3 planePosition,
            Vector3 planeForward,
            PlaneSpec spec,
            float maxDuration = 2.0f,
            float sampleInterval = 0.1f
        )
        {
            samples = new List<Vector3>();

            Vector3 dragDir = handleDirection.normalized;
            float dragAngle = Vector3.SignedAngle(planeForward, dragDir, Vector3.up); // +이면 좌회전

            float maxTurnAngle = spec.maxTurnRateDeg * maxDuration;
            float clampedAngle = Mathf.Clamp(dragAngle, -maxTurnAngle, maxTurnAngle);

            float clampedDistance = Mathf.Clamp(handleDistance, 0f, spec.maxSpeed * maxDuration);
            float estimatedDuration = clampedDistance / spec.maxSpeed;

            float g = 9.81f;
            float radius = (spec.maxSpeed * spec.maxSpeed) / (g * spec.maxGForce);

            if (radius < 0.1f || float.IsNaN(radius))
                return;

            int samplesCount = Mathf.CeilToInt(estimatedDuration / sampleInterval);
            float anglePerStep = clampedAngle / samplesCount;
            float arcLength = Mathf.Deg2Rad * Mathf.Abs(clampedAngle) * radius;
            float arcStepLength = arcLength / samplesCount;

            Vector3 dir = planeForward.normalized;
            Vector3 pos = planePosition;

            for (int i = 0; i <= samplesCount; i++)
            {
                samples.Add(pos);

                Quaternion q = Quaternion.Euler(0f, anglePerStep, 0f);
                dir = q * dir;
                pos += dir * arcStepLength;
            }
        }

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