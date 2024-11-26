using Unity.Netcode;
using UnityEngine;

//menu

public class ClenupMenu : MonoBehaviour
{
    void Awake()
    {
        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);

        if (MultiplayerManager.Instance != null)
            Destroy(MultiplayerManager.Instance.gameObject);

        if (LobbyManager.Instance != null)
            Destroy(LobbyManager.Instance.gameObject);
    }
}
