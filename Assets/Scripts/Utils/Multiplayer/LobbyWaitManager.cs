using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyWaitManager : NetworkBehaviour
{
	//aa
    public static LobbyWaitManager Instance { get; private set; }

	public event EventHandler OnReadyChanged;

    [SerializeField] private Dictionary<ulong, bool> m_playerReadyDictionary;

    void Awake()
    {
        Instance = this;
        m_playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
	private void SetPlayerReadyServerRpc(ServerRpcParams p_serverRpcParams = default)
	{
		SetPlayerReadyClientRpc(p_serverRpcParams.Receive.SenderClientId);
		m_playerReadyDictionary[p_serverRpcParams.Receive.SenderClientId] = true;

		bool l_allClientsReady = true;
		foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
		{
			if (!m_playerReadyDictionary.ContainsKey(clientId) || !m_playerReadyDictionary[clientId])
			{
				// this player is NOT ready
				l_allClientsReady = false;
				break;
			}
		}

		if (l_allClientsReady)
		{
			Debug.Log("[GAME] all clients are ready");
			SceneLoader.LoadNetwork(SceneLoader.Scene.SCN_Game);
			LobbyManager.Instance.DeleteLobby();
		}
	}

	[ClientRpc]
	private void SetPlayerReadyClientRpc(ulong p_clientId)
	{
		m_playerReadyDictionary[p_clientId] = true;
		OnReadyChanged?.Invoke(this, EventArgs.Empty);
	}

	public bool IsPlayerReady(ulong p_clientId)
	{
		return m_playerReadyDictionary.ContainsKey(p_clientId) && m_playerReadyDictionary[p_clientId];
	}
}
