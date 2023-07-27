using System;
using UnityEngine;

// ReSharper disable StaticMemberInGenericType

namespace GT.Core
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool _quitting;
        private static readonly Lazy<T> LazyInstance = new(Create);

        private static T Create()
        {
            var instance = (T)FindObjectOfType(typeof(T));

            if (instance != null) return instance;

            var singletonObject = new GameObject("[" + typeof(T).Name + "] (Singleton)");
            instance = singletonObject.AddComponent<T>();
            DontDestroyOnLoad(singletonObject);

            return instance;
        }

        public static T Instance
            => _quitting ? null : LazyInstance.Value;

        static Singleton()
        {
            Application.quitting += () => _quitting = true;
        }

        private void OnDestroy()
        {
            _quitting = true;
        }
    }
}