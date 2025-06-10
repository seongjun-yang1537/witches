using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Corelib.Utils
{
    [CustomEditor(typeof(SceneAsset))]
    public class EditorSceneAsset : InnerEditor<SceneAsset>
    {
        private GameObject scenePrefab;
        private Dictionary<Editor, bool> activeEditors = new();

        public override void OnEnable()
        {
            base.OnEnable();
            string assetPath = AssetDatabase.GetAssetPath(target);
            scenePrefab = ScenePrefabUtility.GetScenePrefab(assetPath);

            if (scenePrefab == null)
            {
                scenePrefab = ScenePrefabUtility.CreateScenePrefab(assetPath);
            }

            InitActiveEditors();
            Undo.undoRedoPerformed += InitActiveEditors;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = true;
            DrawInspectors();
            ProcessComponentEvent();
            DrawAddComponent();
        }

        public override void OnDisable()
        {
            Undo.undoRedoPerformed -= InitActiveEditors;
            ClearActiveEditors();
            AssetDatabase.SaveAssets();
        }

        private void DrawAddComponent()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Component", GUILayout.Width(230), GUILayout.Height(25)))
                {

                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawInspectors()
        {
            List<Editor> editors = new(activeEditors.Keys);

            foreach (Editor editor in editors)
            {
                if (editor.target == null)
                {
                    continue;
                }
                EditorGUIDrawer.DrawInspectorTitlebar(editor, activeEditors[editor], (foldOut) =>
                {
                    activeEditors[editor] = foldOut;
                });
                if (activeEditors[editor])
                {
                    GUILayout.Space(-5f);
                    editor.OnInspectorGUI();
                    EditorGUIDrawer.DrawInspectorLine();
                }
            }
            GUILayout.Space(5f);
            EditorGUIDrawer.DrawInspectorLine();
        }

        private void ProcessComponentEvent()
        {
            Rect dragAndDropRect = GUILayoutUtility.GetRect(GUIContent.none,
                                                            GUIStyle.none,
                                                            GUILayout.ExpandHeight(true),
                                                            GUILayout.MinHeight(5));

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (dragAndDropRect.Contains(e.mousePosition) == false)
                        {
                            break;
                        }

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (e.type == EventType.DragPerform)
                        {
                            var compoenents = DragAndDrop.objectReferences
                            .Where(x => x.GetType() == typeof(MonoScript))
                            .OfType<MonoScript>()
                            .Select(m => m.GetClass());

                            foreach (var component in compoenents)
                            {
                                Undo.AddComponent(scenePrefab, component);
                            }

                            InitActiveEditors();
                        }
                    }
                    break;

            }

            GUI.Label(dragAndDropRect, "");
        }

        private void ClearActiveEditors()
        {
            foreach (var activeEditor in activeEditors.Keys)
            {
                DestroyImmediate(activeEditor);
            }
            activeEditors.Clear();
        }

        private void InitActiveEditors()
        {
            ClearActiveEditors();

            var components = scenePrefab.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component is Transform || component is RectTransform)
                {
                    continue;
                }

                activeEditors.Add(Editor.CreateEditor(component), true);
            }
        }
    }
}