using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : NetworkBehaviour
{

	//aa
	public static MultiplayerManager Instance { get; private set; }

	public const int MAX_PLAYER_AMOUNT = 2;
	private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

	public event EventHandler OnTryingToJoinGame;
	public event EventHandler OnFailToJoinGame;
	public event EventHandler OnPlayerDataNetworkListChanged;

	private NetworkList<PlayerData> m_playerDataNetworkList; // network lists HAVE to be initialized on awake
	//[SerializeField] private NetworkVariable<CardList> m_hostCardList;
	//[SerializeField] private NetworkVariable<CardList> m_clientCardList;

	// [SerializeField] private NetworkVariable<PlayerController> m_hostPlayerController;
	// [SerializeField] private NetworkVariable<PlayerController> m_clientPlayerController;
	
	//[SerializeField] private List<PlayerController> m_playerControllerList;

	private string m_playerName;

	void Awake()
	{
		Instance = this;

		DontDestroyOnLoad(gameObject);

		m_playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

		m_playerDataNetworkList = new NetworkList<PlayerData>();

		m_playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
	}

	public string GetPlayerName()
	{
		return m_playerName;
	}

	public void SetPlayerName(string p_playerName)
	{
		m_playerName = p_playerName;

		PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, p_playerName);
	}

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> p_changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
	{
		NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
		NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
		NetworkManager.Singleton.StartHost();
	}

    public void StartClient()
	{
		OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

		NetworkManager.Singleton.OnClientConnectedCallback +=  NetworkManager_Client_OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback +=  NetworkManager_Client_OnClientDisconnectCallback;
		NetworkManager.Singleton.StartClient();
	}

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest p_connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse p_connectionApprovalResponse)
    {

		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneLoader.Scene.SCN_WaitLobby.ToString())
		{
	        p_connectionApprovalResponse.Approved = false;
			p_connectionApprovalResponse.Reason = Localization.Instance.Localize("REASON_GAME_STARTED");
			return;
		}

		if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
		{
			p_connectionApprovalResponse.Approved = false;
			p_connectionApprovalResponse.Reason = Localization.Instance.Localize("REASON_GAME_FULL");
			return;
		}

		p_connectionApprovalResponse.Approved = true;

    }

	private void NetworkManager_Server_OnClientConnectedCallback(ulong p_clientId)
    {
        m_playerDataNetworkList.Add(new PlayerData {
			clientId = p_clientId
		});
        SetPlayerNameServerRpc(GetPlayerName());
	}

	private void NetworkManager_Client_OnClientConnectedCallback(ulong p_clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
    }

	private void NetworkManager_Server_OnClientDisconnectCallback(ulong p_clientId)
	{
		for (int i = 0; i < m_playerDataNetworkList.Count; i++)
		{
			PlayerData l_playeData = m_playerDataNetworkList[i];
			if (l_playeData.clientId == p_clientId)
			{
				// disconnected
				m_playerDataNetworkList.RemoveAt(i);
			}
		}
	}

	private void NetworkManager_Client_OnClientDisconnectCallback(ulong p_clientId)
    {
        OnFailToJoinGame?.Invoke(this, EventArgs.Empty);
    }

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerNameServerRpc(string p_playerName, ServerRpcParams p_serverRpcParams = default)
	{
		int l_playerDataIndex = GetPlayerDataIndexFromClientId(p_serverRpcParams.Receive.SenderClientId);

		PlayerData l_playerData = m_playerDataNetworkList[l_playerDataIndex];

		l_playerData.playerName = p_playerName;

		m_playerDataNetworkList[l_playerDataIndex] = l_playerData;
	}

	public bool IsPlayerIndexConnected(int p_playerIndex)
	{
		return p_playerIndex < m_playerDataNetworkList.Count;
	}

	public int GetPlayerDataIndexFromClientId(ulong p_clientId)
	{
		for (int i = 0; i < m_playerDataNetworkList.Count; i++)
			if (m_playerDataNetworkList[i].clientId == p_clientId)
				return i;

		return -1;
	}

	public PlayerData GetPlayerDataFromClientId(ulong p_clientId)
	{
		foreach (PlayerData l_playeData in m_playerDataNetworkList)
			if (l_playeData.clientId.Equals(p_clientId))
				return l_playeData;

		return default;
	}

	public PlayerData GetPlayerDataFromPlayerIndex(int p_playerIndex)
	{
		return m_playerDataNetworkList[p_playerIndex];
	}

	// public void AddPlayerControllerToList(PlayerController p_playerController)
	// {
	// 	m_playerControllerList.Add(p_playerController);
	// }

	// public PlayerController GetPlayerControllerFromId(int p_id)
	// {
	// 	return m_playerControllerList[p_id];
	// }

}