using System;
using TMPro;
using UnityEngine;

public class PlayerReadyManager : MonoBehaviour
{

    [SerializeField] private int m_playerIndex;
    [SerializeField] private GameObject m_readyGameObject;
    [SerializeField] private TextMeshPro m_playerNameText;

    void Start()
    {
        GameMultiplayerManager.Instance.OnPlayerDataNetworkListChanged += GameMultiplayerManager_OnPlayerDataNetworkListChanged;
        WaitLobbyManager.Instance.OnReadyChanged += WaitLobbyManager_OnReadyChanged;
        UpdatePlayer();
    }

    void OnDestroy()
    {
        GameMultiplayerManager.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayerManager_OnPlayerDataNetworkListChanged;
    }

    private void GameMultiplayerManager_OnPlayerDataNetworkListChanged(object p_sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void WaitLobbyManager_OnReadyChanged(object p_sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (GameMultiplayerManager.Instance.IsPlayerIndexConnected(m_playerIndex))
        {
            Show();
            PlayerData l_playerData = GameMultiplayerManager.Instance.GetPlayerDataFromPlayerIndex(m_playerIndex);
            m_readyGameObject.SetActive(WaitLobbyManager.Instance.IsPlayerReady(l_playerData.clientId));

            m_playerNameText.text = l_playerData.playerName.ToString();
        }
        else
            Hide();
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
