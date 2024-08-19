using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// to give players the option to rejoin, we would need to save their player data (items, cards)
// and sync with the next person who joins (as it is only two, it would work)
// also, we would need to sync the game data, network variables for things or server rpc when someone connect to set variables
// client connect callback to send this data to just that client (what was already spawned and stuff like that)

public class DisconnectUI : MonoBehaviour
{
    [SerializeField] private Button m_mainMenuButton;

    void Awake()
    {
        m_mainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.SCN_Menu);
        });
    }

    void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        Hide();
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (NetworkManager.Singleton != null && clientId == NetworkManager.ServerClientId)
        {
            // server is shutting down
            Show();
        }
        else
            Debug.Log("someone disconnected: " + clientId);
    }

    private void Show()
	{
		gameObject.SetActive(true);
	}

	private void Hide()
    {
        gameObject.SetActive(false);
    }
}
