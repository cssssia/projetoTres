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
        MultiplayerManager.Instance.OnFailToJoinGame += MultiplayerManager_OnFailToJoinGame;
        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed += LobbyManager_OnCreateLobbyFailed;
        LobbyManager.Instance.OnJoinStarted += LobbyManager_OnJoinStarted;
        LobbyManager.Instance.OnQuickJoinFailed += LobbyManager_OnQuickJoinFailed;
        LobbyManager.Instance.OnCodeJoinFailed += LobbyManager_OnCodeJoinFailed;
        Hide();
    }

    void OnDestroy()
    {
        MultiplayerManager.Instance.OnFailToJoinGame -= MultiplayerManager_OnFailToJoinGame;
        LobbyManager.Instance.OnCreateLobbyStarted -= LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed -= LobbyManager_OnCreateLobbyFailed;
        LobbyManager.Instance.OnJoinStarted -= LobbyManager_OnJoinStarted;
        LobbyManager.Instance.OnQuickJoinFailed -= LobbyManager_OnQuickJoinFailed;
        LobbyManager.Instance.OnCodeJoinFailed -= LobbyManager_OnCodeJoinFailed;
    }

    private void MultiplayerManager_OnFailToJoinGame(object p_sender, EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "") // if connection gets timeout, text message is empty
            ShowMessage(Localization.Instance.Localize("MESSAGE_FAILED_CONNECT"));
        else
            ShowMessage(NetworkManager.Singleton.DisconnectReason);

        Show();
    }

    private void LobbyManager_OnCreateLobbyStarted(object p_sender, EventArgs e)
    {
        ShowMessage(Localization.Instance.Localize("MESSAGE_CREATING"));
    }

    private void LobbyManager_OnCreateLobbyFailed(object p_sender, EventArgs e)
    {
        ShowMessage(Localization.Instance.Localize("MESSAGE_FAILED_CREATE_LOBBY"));
    }

    private void LobbyManager_OnJoinStarted(object p_sender, EventArgs e)
    {
        ShowMessage(Localization.Instance.Localize("MESSAGE_JOINING"));
    }

    private void LobbyManager_OnQuickJoinFailed(object p_sender, EventArgs e)
    {
        ShowMessage(Localization.Instance.Localize("MESSAGE_FAILED_QUICK_LOBBY"));
    }

    private void LobbyManager_OnCodeJoinFailed(object p_sender, EventArgs e)
    {
        ShowMessage(Localization.Instance.Localize("MESSAGE_FAILED_JOIN_LOBBY"));
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
