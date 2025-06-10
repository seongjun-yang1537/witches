using UnityEngine;

namespace Ingame
{
    public class AgentView : MonoBehaviour
    {
        public Transform body;
        protected AgentModel agentModel;

        private MeshRenderer[] meshRenderers;

        protected void Awake()
        {
            agentModel = GetComponent<AgentModel>();
            meshRenderers = body.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
        }

        protected void Start()
        {
            InitializeTeamMaterial();
        }

        private void InitializeTeamMaterial()
        {
            Material baseMaterial = MaterialDB.Get("TeamMaterial");
            foreach (var renderer in meshRenderers)
            {
                Material teamMaterial = new Material(baseMaterial);
                teamMaterial.SetColor("_Color", AgentTeamDB.TEAM_COLOR[agentModel.team]);

                renderer.material = teamMaterial;
            }
        }
    }
}