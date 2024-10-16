using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_messageText;
    [SerializeField] private Button m_closeButton;

    void Awake()
    {
        m_closeButton.onClick.AddListener(Hide);
    }

    void Start()
    {
        GameMultiplayerManager.Instance.OnFailToJoinGame += GameMultiplayerManager_OnFailToJoinGame;
        GameLobby.Instance.OnCreateLobbyStarted += GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnCreateLobbyFailed += GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnJoinStarted += GameLobby_OnJoinStarted;
        GameLobby.Instance.OnQuickJoinFailed += GameLobby_OnQuickJoinFailed;
        GameLobby.Instance.OnCodeJoinFailed += GameLobby_OnCodeJoinFailed;
        Hide();
    }

    void OnDestroy()
    {
        GameMultiplayerManager.Instance.OnFailToJoinGame -= GameMultiplayerManager_OnFailToJoinGame;
        GameLobby.Instance.OnCreateLobbyStarted -= GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnCreateLobbyFailed -= GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnJoinStarted -= GameLobby_OnJoinStarted;
        GameLobby.Instance.OnQuickJoinFailed -= GameLobby_OnQuickJoinFailed;
        GameLobby.Instance.OnCodeJoinFailed -= GameLobby_OnCodeJoinFailed;
    }

    private void GameMultiplayerManager_OnFailToJoinGame(object p_sender, EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "") // if connection gets timeout, text message is empty
            ShowMessage(ErrorMessage.MESSAGE_FAILED_CONNECT);
        else
            ShowMessage(NetworkManager.Singleton.DisconnectReason);

        Show();
    }

    private void GameLobby_OnCreateLobbyStarted(object p_sender, EventArgs e)
    {
        ShowMessage(ErrorMessage.MESSAGE_CREATING);
    }

    private void GameLobby_OnCreateLobbyFailed(object p_sender, EventArgs e)
    {
        ShowMessage(ErrorMessage.MESSAGE_FAILED_CREATE_LOBBY);
    }

    private void GameLobby_OnJoinStarted(object p_sender, EventArgs e)
    {
        ShowMessage(ErrorMessage.MESSAGE_JOINING);
    }

    private void GameLobby_OnQuickJoinFailed(object p_sender, EventArgs e)
    {
        ShowMessage(ErrorMessage.MESSAGE_FAILED_QUICK_LOBBY);
    }

    private void GameLobby_OnCodeJoinFailed(object p_sender, EventArgs e)
    {
        ShowMessage(ErrorMessage.MESSAGE_FAILED_JOIN_LOBBY);
    }

    private void ShowMessage(string p_message)
    {
        Show();
        m_messageText.text = p_message;
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
