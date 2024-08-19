using UnityEngine;

public abstract class Singleton<T> : Singleton where T : MonoBehaviour
{
    public static T Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this as T;
        else Destroy(this.gameObject);
    }
}

public abstract class Singleton : MonoBehaviour
{
    #region  Properties
    public static bool Quitting { get; protected set; }
    #endregion

    #region  Methods
    private void OnApplicationQuit()
    {
        Quitting = true;
    }
    #endregion
}
