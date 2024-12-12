using UnityEngine;
using FishNet.Object;

public class AbstractNetworkSingleton<T> : NetworkBehaviour where T : Component
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance != null) return instance;
            
            instance = FindObjectOfType<T>();
            return instance;
        }
    }
}
