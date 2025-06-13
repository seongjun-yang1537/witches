using System.Collections.Generic;
using System.Linq;
using Corelib.Utils;
using Sirenix.Serialization;
using UnityEngine;

namespace Ingame
{
    public class AgentModel : MonoBehaviour
    {
        public AgentTeam team;
        public AgentType agentType;
        public float life;
        public float lifeMax;

        public float range;
        public float attackPower;
        public float attackSpeed;

        public float speed;

        // TODO: 전투기, 조종사로 데이터 구조 나누기
        public Vector3 targetPosition { get; private set; }
        private Vector3 originPosition;

        protected GameManager gameManager { get => GameManager.Instance; }

        [SerializeField]
        private float attackDelay;

        protected void Awake()
        {

        }

        protected void Start()
        {
            InitializeTargetPosition();
            life = lifeMax;
        }

        private void UpdateAttack()
        {
            if (attackDelay > 0f)
                return;
            AgentModel aimtarget = FindAimtarget();
            if (aimtarget == null) return;

            Shoot(aimtarget);
            attackDelay = attackSpeed;
        }

        public virtual void OnMovePhase()
        {
            InitializeTargetPosition();
        }

        protected virtual AgentModel FindAimtarget()
        {
            List<AgentModel> enemies = gameManager.agentModels.Where(agent => agent.team != team).ToList();
            if (enemies.Count == 0)
                return null;

            Vector3 position = transform.position;
            List<AgentModel> candidates = enemies.Where(enemy =>
            {
                Vector3 enemyPosition = enemy.transform.position;
                return (enemyPosition - position).sqrMagnitude < range * range;
            })
            .ToList();
            if (candidates.Count == 0)
                return null;

            return candidates.Choice();
        }

        public void Shoot(AgentModel target)
        {
            ProjectileContext context = new ProjectileContextBuilder()
                .SetOwner(this)
                .SetTarget(target)
                .SetType("Default")
                .SetDamage(attackPower)
                .SetPosition(transform.position)
                .SetSpeed(3.0f)
                .Build();

            ProjectileManager.Instance.Shoot(context);
        }

        public virtual void OnMovePhaseProgress(float progress)
        {
            Vector3 delta = (targetPosition - transform.position) * progress;
            transform.position = originPosition + delta;

            if (delta.sqrMagnitude > 0f)
                transform.rotation = Quaternion.LookRotation(delta);

            UpdateAttack();
            attackDelay -= Time.deltaTime;
        }

        public virtual void OnAttackPhase()
        {

        }

        public virtual void OnAttackPhaseProgress(float progress)
        {

        }

        private void InitializeTargetPosition()
        {
            Vector3 forward = transform.forward;
            SetTargetPosition(transform.position + 5 * forward);
        }

        public void SetTargetPosition(Vector3 position)
        {
            originPosition = transform.position;
            targetPosition = position;
        }

        public virtual void TakeDamage(AgentModel enemy, float damage)
        {
            life -= damage;
            if (life <= 0f) OnDead();
        }

        public void OnDead()
        {
            gameManager.DeadAgent(this);
            Destroy(gameObject);
        }
    }
}