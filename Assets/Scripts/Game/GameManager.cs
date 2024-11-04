using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
	public static GameManager Instance { get; private set;}

	public event EventHandler OnStateChanged;
	public event EventHandler OnLocalGamePaused;
	public event EventHandler OnLocalGameUnpaused;
	public event EventHandler OnMultiplayerGamePaused;
	public event EventHandler OnMultiplayerGameUnpaused;
	public event EventHandler OnLocalPlayerReadyChanged;

	public enum GameState
	{
		WaitingToStart,
		//CountdownToStart, //check if we will use something like this
		DealingCards,
		HostPlayerTurn,
		ClientPlayerTurn,
		//GamePlaying,
		GameOver,
	}

	[SerializeField] private Transform m_playerPrefab;

	[SerializeField] private NetworkVariable<GameState> m_gameState = new NetworkVariable<GameState>(GameState.WaitingToStart);
	public NetworkVariable<GameState> gameState => m_gameState;
	//private NetworkVariable<float> m_countdownToStartTimer = new NetworkVariable<float>(3f);
	private bool m_isLocalPlayerReady;
	private bool m_isLocalGamePaused = false;
	private NetworkVariable<bool> m_isGamePaused = new NetworkVariable<bool>(false);
	private Dictionary<ulong, bool> m_playerReadyDictionary;
	private Dictionary<ulong, bool> m_playerPauseDictionary;
	private bool m_autoTestGamePauseState;

	void Awake()
	{
		Instance = this;
		m_playerReadyDictionary = new Dictionary<ulong, bool>();
		m_playerPauseDictionary = new Dictionary<ulong, bool>();
	}

	void Start()
	{
		GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
		GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
	}

    public override void OnNetworkSpawn()
    {
        m_gameState.OnValueChanged += GameState_OnValueChanged;
		m_isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

		if (IsServer)
		{
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
			NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted; //triggered on all the clients have loaded the final scene
	        RoundManager.Instance.OnCardPlayed += TurnManager_OnCardPlayed;
			RoundManager.Instance.OnTrickWon += RoundManager_OnTrickWon;
			RoundManager.Instance.RoundHasStarted.OnValueChanged += MatchHasStarted_OnValueChanged;
		}
    }

	private Player m_wonTrickPlayer;
    private void RoundManager_OnTrickWon(object l_wonTrickPlayer, EventArgs e)
    {
        m_wonTrickPlayer = (Player)l_wonTrickPlayer;
    }

    private void MatchHasStarted_OnValueChanged(bool previousValue, bool newValue)
    {
        if (!newValue)
			m_gameState.Value = GameState.DealingCards;
    }

    private void TurnManager_OnCardPlayed(object p_customSender, EventArgs e)
    {
		CustomSender l_customSender = (CustomSender)p_customSender;
		Player l_playerType = (Player)l_customSender.playerType;

		if (RoundManager.Instance.RoundHasStarted.Value)
		{
			if (m_wonTrickPlayer == Player.DEFAULT) //logic turn flow
			{
				if (l_playerType == Player.HOST)
					m_gameState.Value = GameState.ClientPlayerTurn;
				else if (l_playerType == Player.CLIENT)
					m_gameState.Value = GameState.HostPlayerTurn;
			}
			else //trick win
			{
				if (m_wonTrickPlayer == Player.HOST)
					m_gameState.Value = GameState.HostPlayerTurn;
				else if (m_wonTrickPlayer == Player.CLIENT)
					m_gameState.Value = GameState.ClientPlayerTurn;
				else if (m_wonTrickPlayer == Player.DRAW)
				{
					if ((Player)RoundManager.Instance.WhoStartedRound.Value == Player.HOST)
						m_gameState.Value = GameState.HostPlayerTurn;
					else if ((Player)RoundManager.Instance.WhoStartedRound.Value == Player.CLIENT)
						m_gameState.Value = GameState.ClientPlayerTurn;
				}

				m_wonTrickPlayer = Player.DEFAULT;
			}

		}
    }

    private void SceneManager_OnLoadEventCompleted(string p_sceneName, UnityEngine.SceneManagement.LoadSceneMode p_loadSceneMode, List<ulong> p_clientsCompleted, List<ulong> p_clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
		{
			Transform l_playerTransform = Instantiate(m_playerPrefab);
			l_playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
			//GameMultiplayerManager.Instance.AddPlayerControllerToList(l_playerTransform.GetComponent<PlayerController>());
		}
    }

    private void GameState_OnValueChanged(GameState p_previousValue, GameState p_newValue)
	{
		OnStateChanged?.Invoke(this, EventArgs.Empty);
	}

	private void IsGamePaused_OnValueChanged(bool p_previousValue, bool p_newValue)
	{
		if (m_isGamePaused.Value)
		{
			Time.timeScale = 0f;
			OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			Time.timeScale = 1f;
			OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
		}
	}

	private void NetworkManager_OnClientDisconnectCallback(ulong p_clientId)
	{
		m_autoTestGamePauseState = true;
	}

    private void GameInput_OnPauseAction(object p_sender, EventArgs e)
	{
		TogglePauseGame();
	}

	private void GameInput_OnInteractAction(object p_sender, EventArgs e)
	{
		if (m_gameState.Value == GameState.WaitingToStart)
		{
			m_isLocalPlayerReady = true;
			OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
			SetPlayerReadyServerRpc();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerReadyServerRpc(ServerRpcParams p_serverRpcParams = default)
	{
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
			m_gameState.Value = GameState.DealingCards;
	}

	void Update()
	{
		if (!IsServer)
			return;

		switch (m_gameState.Value)
		{
			case GameState.WaitingToStart:
				break;
			case GameState.DealingCards:
				CardsManager.Instance.SpawnNewPlayCardsServerRpc();
				m_wonTrickPlayer = Player.DEFAULT;

				if (RoundManager.Instance.MatchWonHistory.Count == 0) //logic round flow
					m_gameState.Value = GameState.HostPlayerTurn;
				else if ((Player)RoundManager.Instance.WhoStartedRound.Value == Player.HOST)
					m_gameState.Value = GameState.ClientPlayerTurn;
				else if ((Player)RoundManager.Instance.WhoStartedRound.Value == Player.CLIENT)
					m_gameState.Value = GameState.HostPlayerTurn;

				break;
			case GameState.HostPlayerTurn:
				// if (loseMatch)
				// 	m_gameState.Value = GameState.GameOver;
				break;
			case GameState.ClientPlayerTurn:
				break;
			case GameState.GameOver:
				break;
		}

	}

	[ServerRpc(RequireOwnership = false)]
	public void EndTurnServerRpc(int p_clientID)
    {
        switch (p_clientID)
        {
			case 0: //host
				break;
			case 1: //outro
				break;
            default:
                break;
        }
    }

	void SetState(GameState p_gameState)
    {
        switch (p_gameState)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.DealingCards:
				//start new match
				//decide quem come�a a proxima partida baseada na ultima partida
				//decidindo isso, set game state pra HostPlayerTurn || ClientPlayerTurn
				break;
            case GameState.HostPlayerTurn:
				//informar o host q ele ta pronto pra jogar
				break;
            case GameState.ClientPlayerTurn:
				//informar o cliente q ele ta pronto pra jogar
				break;
            case GameState.GameOver:
                break;
        }
    }

    void LateUpdate()
	{
		if (m_autoTestGamePauseState)
		{
			m_autoTestGamePauseState = false;
			TestGamePausedState();
		}
	}

	public bool IsWaitingToStart()
	{
		return m_gameState.Value == GameState.WaitingToStart;
	}

	public bool IsDealingCards()
	{
		return m_gameState.Value == GameState.DealingCards;
	}

	public bool IsHostPlayerTurn()
	{
		return m_gameState.Value == GameState.HostPlayerTurn;
	}

	public bool IsClientPlayerTurn()
	{
		return m_gameState.Value == GameState.ClientPlayerTurn;
	}

	public bool IsMyTurn(bool p_isHost)
	{
		return p_isHost ? IsHostPlayerTurn() : IsClientPlayerTurn();
	}

	public bool IsGameOver()
	{
		return m_gameState.Value == GameState.GameOver;
	}

	public bool IsLocalPlayerReady()
	{
		return m_isLocalPlayerReady;
	}

	public void TogglePauseGame()
	{
		m_isLocalGamePaused = !m_isLocalGamePaused;

		if (m_isLocalGamePaused)
		{
			PauseGameServerRpc();
			OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			UnpauseGameServerRpc();
			OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void PauseGameServerRpc(ServerRpcParams p_serverRpcParams = default)
	{
		m_playerPauseDictionary[p_serverRpcParams.Receive.SenderClientId] = true;
		TestGamePausedState();
	}

	[ServerRpc(RequireOwnership = false)]
	private void UnpauseGameServerRpc(ServerRpcParams p_serverRpcParams = default)
	{
		m_playerPauseDictionary[p_serverRpcParams.Receive.SenderClientId] = false;
		TestGamePausedState();
	}

	private void TestGamePausedState()
	{
		foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
		{
			if (m_playerPauseDictionary.ContainsKey(clientId) && m_playerPauseDictionary[clientId])
			{
				// this player is paused
				m_isGamePaused.Value = true;
				return;
			}
		}

		// all players are unpaused
		m_isGamePaused.Value = false;
	}

}