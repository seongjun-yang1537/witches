using UnityEngine;
using UnityEditor;
using Corelib.SUI;

namespace Ingame
{
    [CustomEditor(typeof(MapModel))]
    public class EditorMapModel : Editor
    {
        MapModel script;
        void OnEnable()
        {
            script = (MapModel)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SEditorGUILayout.Vertical("box")
            .Content(
                SEditorGUILayout.Label("[Spawn]")
                .Bold()
                .Align(TextAnchor.MiddleCenter)
                + SEditorGUILayout.Horizontal()
                .Content(
                    SEditorGUILayout.Button("Ally")
                    .OnClick(() => script.SpawnAgent(AgentType.Player))
                    + SEditorGUILayout.Button("Enemy")
                    .OnClick(() => script.SpawnAgent(AgentType.Enemy))
                )
            )
            .Render();
        }
    }
}