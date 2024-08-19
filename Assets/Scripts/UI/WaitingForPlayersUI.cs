using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayersUI : MonoBehaviour
{
    [SerializeField] private Button m_mainMenuButton;
    [SerializeField] private Button m_readyButton;
    [SerializeField] private TextMeshProUGUI m_lobbyNameText;
    [SerializeField] private TextMeshProUGUI m_lobbyCodeText;

    void Awake()
    {

        m_mainMenuButton.onClick.AddListener(() => {
            GameLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.SCN_Menu);
        });

        m_readyButton.onClick.AddListener(() => {
            WaitLobbyManager.Instance.SetPlayerReady();
        });

    }

    void Start()
    {
        Lobby l_lobby = GameLobby.Instance.GetLobby();
        m_lobbyNameText.text = "LOBBY NAME: " + l_lobby.Name;
        m_lobbyCodeText.text = "LOBBY CODE: " + l_lobby.LobbyCode;
    }

}
