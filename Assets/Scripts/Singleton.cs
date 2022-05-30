using UnityEngine;

namespace BilliardGame
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static GameObject _instanceGO;
        private static T _instance;
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    string typeName = typeof(T).Name;
                    _instanceGO = GameObject.Find(typeName);
                    _instance = _instanceGO.GetComponent<T>();
                    if (_instanceGO == null && _instance == null)
                    {
                        _instanceGO = new GameObject();

                        _instanceGO.name = typeName;

                        _instance = _instanceGO.AddComponent<T>();
                    }
                    GameObject.DontDestroyOnLoad(_instanceGO);
                }

                return _instance;
            }
        }

       
        protected virtual void Start()
        { }

        protected virtual void Update()
        { }

        protected virtual void OnDestroy()
        {
            _instanceGO = null;
            _instance = null;
        }

    }
}
