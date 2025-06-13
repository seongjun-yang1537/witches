using System.Collections.Generic;
using System.Linq;
using Corelib.SUI;
using Corelib.Utils;
using UnityEngine;

namespace Ingame
{

    public class MapModel : SimulationBehaviour
    {
        public Transform entities;
        public Transform envirnoment;

        public List<AgentModel> agentModels = new();
        public List<AgentController> agentControllers = new();

        public Vector3Int size;

        protected void Awake()
        {
            base.Awake();

            InitializeGroups();

            agentModels = entities.Cast<Transform>()
                .Select(child => child.GetComponent<AgentModel>())
                .Where(agentModel => agentModel != null)
                .ToList();
            agentControllers = entities.Cast<Transform>()
                .Select(child => child.GetComponent<AgentController>())
                .Where(agentController => agentController != null)
                .ToList();
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