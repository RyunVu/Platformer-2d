using UnityEngine;

public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T: MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get { return _instance; }
    }

    protected virtual void Awake()
    {
        if(_instance == null)
            _instance = this as T;
        else
            Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if(_instance == this)
            _instance = null;
    }
}

