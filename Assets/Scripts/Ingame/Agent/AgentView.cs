using UnityEngine;

namespace Ingame
{
    public class AgentView : MonoBehaviour
    {
        public Transform body;

        private AgentModel _agentModel;
        protected AgentModel AgentModel { get => _agentModel ??= GetComponent<AgentModel>(); }

        private MeshRenderer[] meshRenderers;

        protected void Awake()
        {
            meshRenderers = body.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
        }
    }
}