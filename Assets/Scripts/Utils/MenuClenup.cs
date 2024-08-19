using Unity.Netcode;
using UnityEngine;

public class MenuClenup : MonoBehaviour
{
    void Awake()
    {
        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);

        if (GameMultiplayerManager.Instance != null)
            Destroy(GameMultiplayerManager.Instance.gameObject);

        if (GameLobby.Instance != null)
            Destroy(GameLobby.Instance.gameObject);
    }
}
