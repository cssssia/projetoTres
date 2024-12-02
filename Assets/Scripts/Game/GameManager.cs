using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnBetStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;

    public enum GameState
    {
        WaitingToStart,
        DealingCards,
        DealingItems,
        HostTurn,
        ClientTurn,
        //GamePlaying,
        GameOver,
    }

    public enum BetState
    {
        WaitingToStart,
        HostTurn,
        ClientTurn,
        MaxBet,
    }

    [SerializeField] private Transform m_playerPrefab;

    [SerializeField] private NetworkVariable<GameState> m_gameState = new NetworkVariable<GameState>(GameState.WaitingToStart);
    [SerializeField] private NetworkVariable<GameState> m_nextGameState = new NetworkVariable<GameState>(GameState.WaitingToStart);

    [SerializeField] private List<bool> m_endedDealingCards;
    [SerializeField] private List<bool> m_endedDealingItems;

    [SerializeField] private NetworkVariable<BetState> m_betState = new NetworkVariable<BetState>(BetState.WaitingToStart);
    public NetworkVariable<GameState> gameState => m_gameState;
    public NetworkVariable<GameState> nextGameState => m_nextGameState;
    public NetworkVariable<BetState> betState => m_betState;
    //private NetworkVariable<float> m_countdownToStartTimer = new NetworkVariable<float>(3f);
    private bool m_isLocalPlayerReady;
    private bool m_isLocalGamePaused = false;
    private NetworkVariable<bool> m_isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> m_playerReadyDictionary;
    private Dictionary<ulong, bool> m_playerPauseDictionary;
    [Header("Animating")]
    private Dictionary<ulong, bool> m_playerAnimatingDictionary;
    public NetworkVariable<bool> IsAnyAnimationPlaying = new NetworkVariable<bool>(false);
    private bool m_autoTestGamePauseState;
    private bool debug_itemEveryRound = true;

    void Awake()
    {
        Instance = this;
        m_playerReadyDictionary = new Dictionary<ulong, bool>();
        m_playerPauseDictionary = new Dictionary<ulong, bool>();
        m_playerAnimatingDictionary = new Dictionary<ulong, bool>();

        m_endedDealingCards = new List<bool>() { false, false };
        m_endedDealingItems = new List<bool>() { false, false };
    }

    void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        m_gameState.OnValueChanged += GameState_OnValueChanged;
        m_betState.OnValueChanged += BetState_OnValueChanged;
        m_isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted; //triggered on all the clients have loaded the final scene
            RoundManager.Instance.OnCardPlayed += TurnManager_OnCardPlayed;
            RoundManager.Instance.OnBet += TurnManager_OnBet;
            RoundManager.Instance.OnTrickWon += RoundManager_OnTrickWon;
            RoundManager.Instance.RoundHasStarted.OnValueChanged += MatchHasStarted_OnValueChanged;
            RoundManager.Instance.OnEndedDealing += OnEndDealingCards;
            RoundManager.Instance.OnEndedDealingItem += OnEndDealingItem;

            OnSetGameState();
        }
    }

    private void TurnManager_OnBet(object p_isIncrease, EventArgs e)
    {
        if (RoundManager.Instance.CurrentTrick.TrickBetMultiplier == 4)
        {
            SetBetState(BetState.MaxBet);
        }

        if ((bool)p_isIncrease)
        {
            if (m_betState.Value == BetState.HostTurn)
            {
                SetBetState(BetState.ClientTurn);
            }
            else if (m_betState.Value == BetState.ClientTurn)
            {
                SetBetState(BetState.HostTurn);
            }
        }
    }

    private Player m_wonTrickPlayer;
    private void RoundManager_OnTrickWon(object l_wonTrickPlayer, EventArgs e)
    {
        m_wonTrickPlayer = (Player)l_wonTrickPlayer;
    }

    private void MatchHasStarted_OnValueChanged(bool previousValue, bool newValue)
    {
        if (!MatchManager.Instance.MatchHasEnded.Value && !newValue)
            SetGameState(GameState.DealingCards);
        else if (MatchManager.Instance.MatchHasEnded.Value && !newValue)
            SetGameState(GameState.GameOver);
    }

    private void TurnManager_OnCardPlayed(object p_cardIndex, EventArgs e)
    {
        Player l_playerType = CardsManager.Instance.GetCardByIndex((int)p_cardIndex).cardPlayer;

        if (RoundManager.Instance.RoundHasStarted.Value)
        {
            if (m_wonTrickPlayer == Player.DEFAULT) //logic turn flow
            {
                if (l_playerType == Player.HOST)
                    SetGameState(GameState.ClientTurn);
                else if (l_playerType == Player.CLIENT)
                    SetGameState(GameState.HostTurn);
            }
            else //trick win
            {
                if (m_wonTrickPlayer == Player.HOST)
                    SetGameState(GameState.HostTurn);
                else if (m_wonTrickPlayer == Player.CLIENT)
                    SetGameState(GameState.ClientTurn);
                else if (m_wonTrickPlayer == Player.DRAW)
                {
                    if (RoundManager.Instance.CurrentTrick.WhoStartedTrick == Player.HOST)
                        SetGameState(GameState.HostTurn);
                    else if (RoundManager.Instance.CurrentTrick.WhoStartedTrick == Player.CLIENT)
                        SetGameState(GameState.ClientTurn);
                }

                m_wonTrickPlayer = Player.DEFAULT;
            }
            if (RoundManager.Instance.CurrentTrick.TrickBetMultiplier == 1)
            {
                if (IsHostTurn())
                    m_betState.Value = BetState.HostTurn;
                else if (IsClientTurn())
                    m_betState.Value = BetState.ClientTurn;

                Debug.Log("[GAME] Bet: " + m_betState.Value + " Game: " + m_gameState.Value);
            }
        }
    }

    private void SceneManager_OnLoadEventCompleted(string p_sceneName, UnityEngine.SceneManagement.LoadSceneMode p_loadSceneMode, List<ulong> p_clientsCompleted, List<ulong> p_clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform l_playerTransform = Instantiate(m_playerPrefab);
            l_playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            //MultiplayerManager.Instance.AddPlayerControllerToList(l_playerTransform.GetComponent<PlayerController>());
        }
    }

    private void GameState_OnValueChanged(GameState p_previousValue, GameState p_newValue)
    {
        OnGameStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BetState_OnValueChanged(BetState p_previousValue, BetState p_newValue)
    {
        OnBetStateChanged?.Invoke(this, EventArgs.Empty);
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
        if (IsWaitingToStart())
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
            SetGameState(GameState.DealingCards);
    }

    void OnSetGameState()
    {
        if (!IsServer)
            return;

        switch (m_gameState.Value)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.DealingCards:
                CardsManager.Instance.DealPlayCardsServerRpc();
                m_wonTrickPlayer = Player.DEFAULT;

                if (debug_itemEveryRound && !CardsManager.Instance.BothPlayersHaveItem)
                {
                    SetNextGameState(GameState.DealingItems);
                }
                else if (RoundManager.Instance.RoundWonHistory.Count == 0) //logic round flow
                {
                    SetNextGameState(GameState.HostTurn);
                }
                else if (RoundManager.Instance.RoundWonHistory.Count > 0 && RoundManager.Instance.RoundWonHistory.Count % 3 == 0 && !CardsManager.Instance.BothPlayersHaveItem)
                {
                    SetNextGameState(GameState.DealingItems);
                }
                else SetNextGameStateToPlayers();
                break;
            case GameState.DealingItems:
                CardsManager.Instance.DealItemsToPlayersServerRpc();
                SetNextGameStateToPlayers();
                break;
            case GameState.HostTurn:
                break;
            case GameState.ClientTurn:
                break;
            case GameState.GameOver:
                break;
        }

    }

    void SetNextGameStateToPlayers()
    {
        if (RoundManager.Instance.RoundWonHistory.Count == 0)
        {
            SetNextGameState(GameState.HostTurn);
        }
        else if (RoundManager.Instance.CurrentTrick.WhoStartedTrick == Player.HOST)
        {
            SetNextGameState(GameState.ClientTurn);
        }
        else if (RoundManager.Instance.CurrentTrick.WhoStartedTrick == Player.CLIENT)
        {
            SetNextGameState(GameState.HostTurn);
        }
    }

    public void OnEndDealingCards(object p_index, EventArgs p_args)
    {
        if (m_gameState.Value is not GameState.DealingCards) return;

        m_endedDealingCards[(int)p_index] = true;
        if (!m_endedDealingCards[0] || !m_endedDealingCards[1]) return;

        if (m_nextGameState.Value is GameState.HostTurn)
        {
            SetGameState(GameState.HostTurn);
            SetBetState(BetState.HostTurn);
        }
        else if (m_nextGameState.Value is GameState.ClientTurn)
        {
            SetGameState(GameState.ClientTurn);
            SetBetState(BetState.ClientTurn);
        }
        else if (m_nextGameState.Value is GameState.DealingItems)
        {
            SetGameState(GameState.DealingItems);
            SetBetState(BetState.WaitingToStart);
        }
    }

    public void OnEndDealingItem(object p_index, EventArgs p_args)
    {
        if (m_gameState.Value is not GameState.DealingItems) return;

        m_endedDealingItems[(int)p_index] = true;
        if (!m_endedDealingItems[0] || !m_endedDealingItems[1]) return;

        if (m_nextGameState.Value is GameState.HostTurn) //logic round flow
        {
            SetGameState(GameState.HostTurn);
            SetBetState(BetState.HostTurn);
        }
        else if (m_nextGameState.Value is GameState.ClientTurn)
        {
            SetGameState(GameState.ClientTurn);
            SetBetState(BetState.ClientTurn);
        }
    }


    public void SetGameState(GameState p_gameState)
    {
        if (!IsServer)
        {
            Debug.Log("[ERROR] Client cannot set Network Variables");
            return;
        }

        m_gameState.Value = p_gameState;
        OnSetGameState();
    }

    public void SetBetState(BetState p_betState)
    {
        if (!IsServer)
        {
            Debug.Log("[ERROR] Client cannot set Network Variables");
            return;
        }

        m_betState.Value = p_betState;
    }

    public void SetNextGameState(GameState p_gameState)
    {
        if (!IsServer)
        {
            Debug.Log("[ERROR] Client cannot set Network Variables");
            return;
        }

        m_nextGameState.Value = p_gameState;
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

    public bool IsHostTurn()
    {
        return m_gameState.Value == GameState.HostTurn;
    }

    public bool IsClientTurn()
    {
        return m_gameState.Value == GameState.ClientTurn;
    }

    public bool IsMyTurn(bool p_isHost)
    {
        return p_isHost ? IsHostTurn() : IsClientTurn();
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

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerAnimatingServerRpc(bool p_set, ServerRpcParams p_serverRpcParams = default)
    {
        m_playerAnimatingDictionary[p_serverRpcParams.Receive.SenderClientId] = p_set;
        TestCallActionsOnWait();
    }

    [Command]
    public static void playerAnimating(ulong id)
    {
        Debug.Log(Instance.m_playerAnimatingDictionary[id]);
    }


    void TestCallActionsOnWait()
    {
        bool l_stillAnimating = false;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (m_playerAnimatingDictionary.TryGetValue(clientId, out bool l_animating) && l_animating)
            {
                // this player is paused
                l_stillAnimating = true;
                break;
            }
        }

        IsAnyAnimationPlaying.Value = l_stillAnimating;
    }
}