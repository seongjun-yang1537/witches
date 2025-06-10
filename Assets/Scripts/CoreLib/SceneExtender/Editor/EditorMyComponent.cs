#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Corelib.Utils
{
    [CustomEditor(typeof(MyComponent))]
    public class MyComponentEditor : UnityEditor.Editor
    {
        private UnityEditor.IMGUI.Controls.AdvancedDropdownState dropdownState;

        public override void OnInspectorGUI()
        {
            Rect controlRect = UnityEditor.EditorGUILayout.GetControlRect();
            Rect buttonRect = controlRect;

            controlRect.width -= 30;
            UnityEditor.EditorGUI.PropertyField(controlRect, serializedObject.FindProperty("levelName"));

            buttonRect.xMin = controlRect.xMax + 4;
            buttonRect.height -= 1;

            if (GUI.Button(buttonRect, new GUIContent(".."), UnityEditor.EditorStyles.miniButton))
            {
                if (dropdownState == null)
                    dropdownState = new UnityEditor.IMGUI.Controls.AdvancedDropdownState();

                Debug.Log($"path {Application.dataPath}");
                var dropdown = new FileDropdown(dropdownState, Application.dataPath, OnFileSelected)
                {
                    rootName = "My Custom Files & Folders",
                    fileFilter = "*.txt"
                };

                dropdown.Show(buttonRect);
            }

            Rect rect = EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button("hi"))
                {
                    var dropdown = new ComponentDropDown(dropdownState);

                    dropdown.Show(rect);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void OnFileSelected(FileDropdown.CallbackInfo info)
        {
            serializedObject.Update();
            serializedObject.FindProperty("levelName").stringValue = info.fullName;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif