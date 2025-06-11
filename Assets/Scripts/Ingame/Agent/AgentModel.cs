using UnityEngine;

namespace Ingame
{
    public class AgentModel : MonoBehaviour
    {
        public AgentTeam team;
        public AgentType agentType;
        public float life;
        public float lifeMax;

        public float speed;

        // TODO: 전투기, 조종사로 데이터 구조 나누기
        public Vector3 targetPosition;
        private Vector3 originPosition;

        protected void Awake()
        {

        }

        protected void Start()
        {
            InitializeTargetPosition();
        }

        public virtual void OnPhase()
        {
            InitializeTargetPosition();
        }

        public virtual void OnMovePhase(float progress)
        {
            Vector3 delta = (targetPosition - transform.position) * progress;
            transform.position = originPosition + delta;

            if (delta.sqrMagnitude > 0f)
                transform.rotation = Quaternion.LookRotation(delta);
        }

        public virtual void OnAttackPhase(float progress)
        {

        }

        private void InitializeTargetPosition()
        {
            Vector3 forward = transform.forward;
            originPosition = transform.position;
            targetPosition = originPosition + 5 * forward;
        }
    }
}