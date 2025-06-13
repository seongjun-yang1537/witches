using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    [Serializable]
    public class PlaneSpec
    {
        public float maxSpeed = 2f;
        public float minSpeed = 0.5f;
        public float maxTurnRateDeg = 90f;  // deg/sec
        public float maxGForce = 5f;
    }

    public class AgentModel : MonoBehaviour
    {
        public UnityEvent onSelected = new();
        public UnityEvent onDeSelected = new();

        [SerializeField]
        public PlaneSpec planeSpec;

        public AgentTeam team;
        public AgentType agentType;
        public float life;
        public float lifeMax;

        public float speed;

        public AgentModel aimtarget;
        public Vector3 aimDirection;

        [SerializeField]
        public Trajectory trajectory { get; private set; }

        public Vector3 targetPosition { get; private set; }

        protected void Start()
        {
            life = lifeMax;
        }

        public void SetTargetPosition(Vector3 targetPosition)
        {
            this.targetPosition = targetPosition;

            Vector3 handleSubVec = targetPosition - transform.position;
            Vector3 handleDirection = handleSubVec.normalized;
            float handleDistance = handleSubVec.magnitude;

            trajectory = new(
                handleDirection,
                handleDistance,
                transform.position,
                transform.forward,
                planeSpec
            );
        }
    }
}