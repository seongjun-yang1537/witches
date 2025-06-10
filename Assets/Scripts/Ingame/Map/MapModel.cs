using Corelib.SUI;
using Corelib.Utils;
using UnityEngine;

namespace Ingame
{

    public class MapModel : MonoBehaviour
    {
        public Transform entities;
        public Transform envirnoment;

        public Vector3Int size;
        private MT19937 rng { get => GameManager.Instance.rng; }

        protected void Awake()
        {
            InitializeGroups();
        }

        private void InitializeGroups()
        {
            entities = FindDirectChildByName(nameof(entities));
            envirnoment = FindDirectChildByName(nameof(envirnoment));
        }

        private Transform FindDirectChildByName(string name)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name == name)
                    return child;
            }
            return null;
        }

        private Vector3 SelectRandomSpawnPosition()
            => transform.localToWorldMatrix.MultiplyPoint(
                new Vector3(
                rng.NextFloat(-size.x / 2, size.x / 2),
                rng.NextFloat(-size.y / 2, size.y / 2),
                rng.NextFloat(-size.z / 2, size.z / 2)
            ));

        public void SpawnAgent(AgentType agentType)
        {
            GameObject go = Instantiate(AgentDB.Get(agentType));

            Transform tr = go.transform;
            tr.SetParent(entities);
            tr.position = SelectRandomSpawnPosition();

            AgentModel agentModel = go.GetComponent<AgentModel>();
            go.name = $"[{agentModel.team}]{agentType}";
        }

        private void OnDrawGizmos()
        {
            SGizmos.Scope(
                SGizmos.WireCube(Vector3.zero, size)
            )
            .Color(Color.blue)
            .Matrix(transform.localToWorldMatrix)
            .Render();
        }
    }
}