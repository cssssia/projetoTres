using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

// TODO handle code empty

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button m_mainMenuButton;
    [SerializeField] private Button m_createLobbyButton;
    [SerializeField] private Button m_quickJoinButton;
    [SerializeField] private Button m_codeJoinButton;
    [SerializeField] private TMP_InputField m_lobbyCodeInputField;
    [SerializeField] private TMP_InputField m_playerNameInputField;
    [SerializeField] private LobbyCreateUI m_lobbyCreateUI;
    [SerializeField] private Transform m_lobbyContainer;
    [SerializeField] private Transform m_lobbyTemplate;

    void Awake()
    {

        m_mainMenuButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.SCN_Menu);
        });

        m_createLobbyButton.onClick.AddListener(() => {
            m_lobbyCreateUI.Show();
        });

        m_quickJoinButton.onClick.AddListener(() => {
            LobbyManager.Instance.QuickJoin();
        });

        m_codeJoinButton.onClick.AddListener(() => {
            LobbyManager.Instance.CodeJoin(m_lobbyCodeInputField.text);
        });

        m_lobbyTemplate.gameObject.SetActive(false);

    }

    void Start()
    {
        m_playerNameInputField.text = MultiplayerManager.Instance.GetPlayerName();
        m_playerNameInputField.onValueChanged.AddListener((string p_newText) => {
            MultiplayerManager.Instance.SetPlayerName(p_newText);
        });

        LobbyManager.Instance.OnLobbyListChanged += GammeLobby_OnLobbyListChanged;

        UpdateLobbyList(new List<Lobby>());
    }

    void OnDestroy()
    {
        LobbyManager.Instance.OnLobbyListChanged -= GammeLobby_OnLobbyListChanged;
    }

    private void GammeLobby_OnLobbyListChanged(object p_sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> p_lobbyList)
    {
        foreach (Transform child in m_lobbyContainer)
        {
            if (child == m_lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in p_lobbyList)
        {
            Transform l_lobbyTransform = Instantiate(m_lobbyTemplate, m_lobbyContainer);
            l_lobbyTransform.gameObject.SetActive(true);
            l_lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

}
