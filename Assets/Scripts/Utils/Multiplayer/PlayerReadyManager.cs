using System;
using TMPro;
using UnityEngine;

public class PlayerReadyManager : MonoBehaviour
{
    //aa
    [SerializeField] private int m_playerIndex;
    [SerializeField] private GameObject m_readyGameObject;
    [SerializeField] private TextMeshPro m_playerNameText;

    void Start()
    {
        MultiplayerManager.Instance.OnPlayerDataNetworkListChanged += MultiplayerManager_OnPlayerDataNetworkListChanged;
        LobbyWaitManager.Instance.OnReadyChanged += LobbyWaitManager_OnReadyChanged;
        UpdatePlayer();
    }

    void OnDestroy()
    {
        MultiplayerManager.Instance.OnPlayerDataNetworkListChanged -= MultiplayerManager_OnPlayerDataNetworkListChanged;
    }

    private void MultiplayerManager_OnPlayerDataNetworkListChanged(object p_sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void LobbyWaitManager_OnReadyChanged(object p_sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (MultiplayerManager.Instance.IsPlayerIndexConnected(m_playerIndex))
        {
            Show();
            PlayerData l_playerData = MultiplayerManager.Instance.GetPlayerDataFromPlayerIndex(m_playerIndex);
            m_readyGameObject.SetActive(LobbyWaitManager.Instance.IsPlayerReady(l_playerData.clientId));

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
