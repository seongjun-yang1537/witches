using UnityEngine;
using UnityEditor;
using Corelib.SUI;

namespace Ingame
{
    [CustomEditor(typeof(GameManager))]
    public class EditroGameManager : Editor
    {
        GameManager script;
        private void OnEnable()
        {
            script = (GameManager)target;
        }
    }
}