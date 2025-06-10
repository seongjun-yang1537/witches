using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;

namespace Corelib.Utils
{
    public class ScenePrefabUtility
    {
        const string PATH_PREFAB_FOLDER = "Assets/Scripts/Tools/CoreLib/SceneExtender/ScenePrefabs";

        [InitializeOnLoadMethod]
        static void OnCreateFolder()
        {
            if (!Directory.Exists(PATH_PREFAB_FOLDER))
            {
                Directory.CreateDirectory(PATH_PREFAB_FOLDER);
            }
        }

        [PostProcessScene]
        static void OnPostProcessScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            string scenePath = scene.path;

            if (string.IsNullOrEmpty(scenePath))
            {
                return;
            }

            GameObject prefab = GetScenePrefab(scenePath);
            if (prefab)
            {
                GameObject.Instantiate(prefab).name = scene.name;
            }
        }

        public static GameObject CreateScenePrefab(string scenePath, params System.Type[] compoenents)
        {
            string guid = ScenePathToGUID(scenePath);
            GameObject go = EditorUtility.CreateGameObjectWithHideFlags(
                guid, HideFlags.None, compoenents);

            string prefabPath = $"{PATH_PREFAB_FOLDER}/{guid}.prefab";
            ValidatePath(prefabPath);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject GetScenePrefab(string scenePath)
        {
            string guid = ScenePathToGUID(scenePath);
            string prefabPath = $"{PATH_PREFAB_FOLDER}/{guid}.prefab";
            ValidatePath(prefabPath);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static void ValidatePath(string prefabPath)
        {
            if (!Directory.Exists(PATH_PREFAB_FOLDER))
            {
                Directory.CreateDirectory(PATH_PREFAB_FOLDER);
            }

            if (Directory.Exists(prefabPath))
            {
                Directory.Delete(prefabPath, true);
            }
        }

        private static string ScenePathToGUID(string scenePath)
            => AssetDatabase.AssetPathToGUID(scenePath);
    }
}