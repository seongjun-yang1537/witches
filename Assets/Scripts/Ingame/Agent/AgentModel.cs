using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Ingame
{
    [Serializable]
    public class PlaneSpec
    {
        public float minLength;
        public float maxLength;

        public float maxDegreePer;
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

        public float radius;

        public float attackPower;

        public AgentModel aimtarget { get; private set; }
        public Vector3 aimDirection { get; private set; }

        [SerializeField]
        public Trajectory trajectory { get; private set; }

        public Vector3 targetPosition { get; private set; }

        protected void Start()
        {
            life = lifeMax;
        }

        protected void Update()
        {
            if (life <= 0f)
                gameObject.SetActive(false);
        }

        public void SetAimDirection(Vector3 direction)
        {
            aimDirection = direction.normalized;
        }

        public void SetTargetPosition(Vector3 targetPosition, string type = "")
        {
            Trajectory newTrajectory = new();
            switch (type)
            {
                case "straight":
                    newTrajectory = Trajectory.CreateStraight(transform.position, targetPosition);
                    break;
                default:
                    newTrajectory = Trajectory.CreateLine(
                                    planeSpec,
                                    transform.position,
                                    transform.forward,
                                    targetPosition
                                );
                    break;
            }
            this.trajectory = newTrajectory;
            this.targetPosition = targetPosition;
        }

        public bool IsCanTargetPosition(Vector3 targetPosition)
        {
            Trajectory newTrajectory = Trajectory.CreateLine(
                planeSpec,
                transform.position,
                transform.forward,
                targetPosition
            );
            return !newTrajectory.IsEmpty;
        }
    }
}