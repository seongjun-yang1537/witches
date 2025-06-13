using System.Collections.Generic;
using System.Linq;
using Corelib.SUI;
using Corelib.Utils;
using Sirenix.Utilities;
using UnityEngine;

namespace Ingame
{

    public class Simulation : SimulationBehaviour
    {
        [SerializeField]
        private List<AgentController> agents;
        public Vector3Int size;

        protected int seed;
        public MT19937 rng;

        private Transform entitiesTrnasform;
        private Transform envirnomentTransform;

        protected void Awake()
        {
            base.Awake();
            rng = seed == -1 ? MT19937.Create() : MT19937.Create(seed);

            InitializeTransforms();
        }

        private void InitializeTransforms()
        {
            entitiesTrnasform = transform.FindInChild("entities");
            envirnomentTransform = transform.FindInChild("envirnoment");

            entitiesTrnasform.Cast<Transform>()
                .Select(t => t.gameObject)
                .Where(go => go.HasComponent<AgentController>())
                .ForEach(go => SetupAgentObject(go));
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
            tr.SetParent(entitiesTrnasform);
            tr.position = SelectRandomSpawnPosition();

            SetupAgentObject(go);
        }

        private void SetupAgentObject(GameObject gameObject)
        {
            AgentController agentController = gameObject.GetComponent<AgentController>();
            AgentModel agentModel = agentController.agentModel;
            gameObject.name = $"[{agentModel.team}]{agentModel.agentType}";

            agents.Add(agentController);
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