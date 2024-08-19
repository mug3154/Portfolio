using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{   
    static T _Instance;
    public static T Instance { get { return _Instance; } }


    private void Awake()
    {
        if(_Instance == null)
        {
            _Instance = GetComponent<T>();

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_Instance != null)
        {
            if (_Instance.gameObject == gameObject)
            {
                Destroy(_Instance);
                _Instance = null;
            }
        }

        Destroy(gameObject);
    }
}
