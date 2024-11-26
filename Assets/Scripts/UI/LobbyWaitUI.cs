using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyWait : MonoBehaviour
{
    [SerializeField] private Button m_mainMenuButton;
    [SerializeField] private Button m_readyButton;
    [SerializeField] private TextMeshProUGUI m_lobbyNameText;
    [SerializeField] private TextMeshProUGUI m_lobbyCodeText;

    void Awake()
    {

        m_mainMenuButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.SCN_Menu);
        });

        m_readyButton.onClick.AddListener(() => {
            LobbyWaitManager.Instance.SetPlayerReady();
        });

    }

    void Start()
    {
        Lobby l_lobby = LobbyManager.Instance.GetLobby();
        m_lobbyNameText.text = "LOBBY NAME: " + l_lobby.Name;
        m_lobbyCodeText.text = "LOBBY CODE: " + l_lobby.LobbyCode;
    }

}
