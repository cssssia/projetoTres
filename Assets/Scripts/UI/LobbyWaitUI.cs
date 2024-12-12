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

        if(l_lobby.IsPrivate)
            m_lobbyCodeText.text = Localization.Instance.Localize("WaitLobby.LobbyCode", l_lobby.LobbyCode);
        else
            m_lobbyCodeText.gameObject.SetActive(false);

        m_lobbyNameText.text = Localization.Instance.Localize("WaitLobby.LobbyName", l_lobby.Name);
    }

}
