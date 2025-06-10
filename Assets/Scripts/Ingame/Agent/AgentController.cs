using UnityEngine;

namespace Ingame
{
    [RequireComponent(typeof(AgentModel))]
    public class AgentController : MonoBehaviour
    {
        protected AgentModel agentModel;

        protected void Awake()
        {
            agentModel = GetComponent<AgentModel>();
        }
    }
}