using System;
using UnityEngine;

public interface ISingleton<T>
{
    public static T Instance { get; }
    public abstract void InitializeSingleton();
}

public abstract class Singleton<T> : MonoBehaviour, ISingleton<T> where T : MonoBehaviour
{
    private static T instance;
    public static T Instance => instance;

    public static event Action<T> OnSingletonInitialized;

    protected virtual void OnEnable()
    {
        InitializeSingleton();
    }

    public virtual void InitializeSingleton()
    {
        if (instance != null)
        {
            Debug.LogWarning($"There were multiple instances of {name} in the scene");

            Destroy(gameObject);
            return;
        }

        instance = this as T;
        OnSingletonInitialized?.Invoke(instance);
    }
}