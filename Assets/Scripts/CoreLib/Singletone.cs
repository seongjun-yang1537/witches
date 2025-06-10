using UnityEngine;

namespace Corelib.Utils
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool isApplicationQuitting = false;

        protected virtual bool ShouldPersist => false;

        public static T Instance
        {
            get
            {
                if (isApplicationQuitting)
                {
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"(Singleton) {typeof(T)}";

                            if (((Singleton<T>)(object)_instance).ShouldPersist)
                                DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            isApplicationQuitting = true;
        }
    }
}