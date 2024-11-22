using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using NaughtyAttributes;

// C:\Users\Usuario\AppData\LocalLow\DefaultCompany\projetoTres > Player.log

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set; }

    [SerializeField] private PlayerSpawnData m_spawnData;
    [SerializeField] private CardTarget[] m_cardTargetTransform;
    [SerializeField] private CardsOnHandBehavior m_handBehavior;
    [SerializeField] private BetOnHandBehavior m_betBehavior;
    [SerializeField] private DeckOnTableBehavior m_deckBehavior;
    [SerializeField] private List<int> m_myHand;

    [Header("Game Info")]
    [SerializeField] private GameManager.GameState currentGameState;
    [SerializeField] private GameManager.BetState currentBetState;

    void Start()
    {
        if (!IsOwner)
            return;
    }

    [Header("Debug buttons")]
    public bool debug_useScissors;
    private void Update()
    {
        if (!IsOwner) return;
        if (debug_useScissors)
        {
            UseScissors();
            debug_useScissors = false;
        }
    }

    public bool IsHostPlayer
    {
        get { return PlayerIndex == 0; }
    }
    public bool IsClientPlayer
    {
        get { return PlayerIndex == 1; }
    }

    public int PlayerIndex
    {
        get { return GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId); }
    }

    private bool canPlay;
    private bool canBet;

    public override void OnNetworkSpawn() //research more the difference of this and awake
    {
        if (IsOwner)
        {
            LocalInstance = this;
            m_betBehavior.OnPlayerSpawned(PlayerIndex);
            m_deckBehavior.OnPlayerSpawned();
        }
        m_handBehavior.OnPlayerSpawned(this);

        transform.SetPositionAndRotation(m_spawnData.spawnPosition[PlayerIndex], Quaternion.Euler(m_spawnData.spawnRotation[PlayerIndex]));
        if (IsOwner) CameraController.Instance.SetCamera(PlayerIndex);

        CardsManager.Instance.OnAddCardToMyHand += CardsManager_OnAddCardToMyHand;
        CardsManager.Instance.OnRemoveCardFromMyHand += CardsManager_OnRemoveCardFromMyHand;
        CardsManager.Instance.OnAddItemCardToMyHand += CardsManager_OnAddItemCardToMyHand;

        RoundManager.Instance.OnRoundWon += TurnManager_OnRoundWon;


        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            RoundManager.Instance.OnStartPlayingCard += TurnManager_OnStartPlayingCard;
        }

        if (IsOwner)
        {
            GameInput.Instance.OnMoveMouse += GameInput_OnMoveMouse;
            GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
            GameInput.Instance.OnStopInteractAction += GameInput_OnClickUpMouse;

            m_cardTargetTransform = FindObjectsByType<CardTarget>(FindObjectsSortMode.None);
            for (int i = 0; i < m_cardTargetTransform.Length; i++)
            {
                if (m_cardTargetTransform[i].clientID == PlayerIndex)
                    m_handBehavior.AddTarget(m_cardTargetTransform[i].transform, m_cardTargetTransform[i].targetIndex);
            }
        }

        if (IsHostPlayer) name = "Player_0";
        else if (IsClient) name = "Player_1";

        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        GameManager.Instance.OnBetStateChanged += OnBetStateChanged;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong p_clientId)
    {
        if (p_clientId == OwnerClientId)
        {
            Debug.Log("[INFO] Owner Disconnected");
            // destroy network stuff
        }
    }

    private void CardsManager_OnAddCardToMyHand(object p_cardIndex, EventArgs e)
    {
        Debug.Log((int)p_cardIndex);
        Debug.Log(CardsManager.Instance.GetCardByIndex((int)p_cardIndex).cardPlayer);
        if (IsOwner && CardsManager.Instance.GetCardByIndex((int)p_cardIndex).cardPlayer == (Player)PlayerIndex)
        {
            m_myHand.Add((int)p_cardIndex);
            print($"deal? {m_myHand.Count == 3}");
            SetCardParentServerRpc((int)p_cardIndex, m_myHand.Count == 3);
        }
    }
    private void CardsManager_OnAddItemCardToMyHand(object p_itemTypeAndPlayer, EventArgs e)
    {
        ItemType p_itemType = (((ItemType, int))p_itemTypeAndPlayer).Item1;
        int p_playerId = (((ItemType, int))p_itemTypeAndPlayer).Item2;

        if (IsOwner && p_playerId == PlayerIndex) SetItemCardParentServerRpc(p_itemType);
    }

    private void CardsManager_OnRemoveCardFromMyHand(object p_cardIndex, EventArgs e)
    {
        if (IsOwner && CardsManager.Instance.GetCardByIndex((int)p_cardIndex).cardPlayer == (Player)PlayerIndex)
        {
            for (int i = 0; i < m_myHand.Count; i++)
            {
                if (m_myHand[i] == (int)p_cardIndex)
                {
                    m_myHand.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private void TurnManager_OnStartPlayingCard(object p_customSender, EventArgs e)
    {
        CustomSender l_customSender = (CustomSender)p_customSender;
        AnimCardClientRpc(l_customSender.playerType, l_customSender.targetIndex, l_customSender.cardNO);
    }

    private void TurnManager_OnRoundWon(object p_playerWonId, EventArgs e)
    {
        if (IsOwner)
        {
            m_myHand.Clear();
            m_handBehavior.ResetCardsOnHandBehavior();

            if (PlayerIndex == (int)p_playerWonId)
                Debug.Log("[GAME] You Won!");
        }

        RemoveCardFromGameClientRpc();

        if (IsServer)
            RoundManager.Instance.RoundHasStarted.Value = false;
    }

    Ray l_rayClickDown;
    private void GameInput_OnInteractAction(object p_sender, System.EventArgs e)
    {
        l_rayClickDown = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(l_rayClickDown, out l_mousePosRaycastHit))
        {
            if (l_mousePosRaycastHit.transform != null)
                CheckClickOnObjects(l_mousePosRaycastHit.transform.gameObject);
            else CheckClickOnObjects(null);
        }
    }

    public bool useHover;
    RaycastHit l_mousePosRaycastHit;
    private void GameInput_OnMoveMouse(object p_sender, System.EventArgs e)
    {
        if (useHover)
        {
            Ray l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(l_ray, out l_mousePosRaycastHit))
            {
                CheckHoverOnObject(l_mousePosRaycastHit.transform.gameObject);
            }

            m_handBehavior.UpdateMousePos(Input.mousePosition);
            m_betBehavior.UpdateMousePos(Input.mousePosition);
        }
    }

    private void GameInput_OnClickUpMouse(object p_sender, System.EventArgs e)
    {
        m_handBehavior.CheckClickUp(canPlay, (go, id) => StartAnim(go, id), (go) => ThrowCard(go));
        if (canPlay || RoundManager.Instance.BetHasStarted.Value) m_betBehavior.CheckClickUp(canBet, (go, increase) => IncreaseBet(go, increase));
        if (RoundManager.Instance.BetHasStarted.Value && canBet) m_deckBehavior.CheckClickUp((go) => GiveUp(go));
    }

    private void CheckClickOnObjects(GameObject p_gameObject)
    {
        m_handBehavior.CheckClickObject(p_gameObject);
        m_betBehavior.CheckClickObject(p_gameObject);
    }

    private void CheckHoverOnObject(GameObject p_gameObject)
    {
        m_handBehavior.CheckHoverObject(p_gameObject);
    }

    private void StartAnim(GameObject gameObject, int p_cardIndex)
    {

        RoundManager.Instance.OnStartAnimServerRpc(PlayerIndex, m_handBehavior.CurrentTargetIndex, p_cardIndex, gameObject.GetComponent<NetworkObject>());
    }

    private void ThrowCard(GameObject gameObject)
    {
        Debug.Log("[GAME] Play Card " + gameObject.name);
        if (gameObject.CompareTag("Card"))
        {
            for (int i = 0; i < m_myHand.Count; i++)
            {
                if (CardsManager.Instance.GetCardByIndex(m_myHand[i]).cardName == gameObject.name)
                {

                    if (!RoundManager.Instance.RoundHasStarted.Value)
                        RoundManager.Instance.StartRoundServerRpc(IsHost ? Player.HOST : Player.CLIENT);

                    RoundManager.Instance.PlayCardServerRpc(m_myHand[i], IsHost ? Player.HOST : Player.CLIENT, m_handBehavior.CurrentTargetIndex);
                    m_myHand.RemoveAt(i);
                }
            }
        }
    }

    private void IncreaseBet(GameObject gameObject, bool increase)
    {
        if (gameObject.CompareTag("Bet"))
        {
            if (!RoundManager.Instance.RoundHasStarted.Value)
                RoundManager.Instance.StartRoundServerRpc(IsHost ? Player.HOST : Player.CLIENT);

            RoundManager.Instance.BetServerRpc(increase);
        }
    }

    private void GiveUp(GameObject gameObject)
    {
        if (gameObject.CompareTag("Deck"))
        {
            RoundManager.Instance.GiveUpServerRpc((Player)PlayerIndex);
        }
    }

    public void OnGameStateChanged(object p_sender, EventArgs p_eventArgs)
    {
        if (!IsOwner) return;

        currentGameState = ((GameManager)p_sender).gameState.Value;
        canPlay = ((currentGameState == GameManager.GameState.HostTurn && IsHostPlayer)
                                            || (currentGameState == GameManager.GameState.ClientTurn && IsClientPlayer))
                                            && !RoundManager.Instance.BetHasStarted.Value;

        if (canPlay) Debug.Log("pode jogar");
        else Debug.Log("não pode jogar");
    }

    public void OnBetStateChanged(object p_sender, EventArgs p_eventArgs)
    {
        if (!IsOwner) return;

        currentBetState = ((GameManager)p_sender).betState.Value;
        canBet = (currentBetState == GameManager.BetState.HostTurn && IsHostPlayer)
                                            || (currentBetState == GameManager.BetState.ClientTurn && IsClientPlayer);

        if (canBet) Debug.Log("pode apostar");
        else Debug.Log("não pode apostar");
    }

    [ServerRpc]
    public void SetCardParentServerRpc(int p_cardIndex, bool p_finishedHandCards)
    {
        SetCardParentClientRpc(p_cardIndex, p_finishedHandCards);
    }

    [ClientRpc]
    public void SetCardParentClientRpc(int p_cardIndex, bool p_finishedHandCards)
    {
        m_handBehavior.AddCardOnHand(p_cardIndex, p_finishedHandCards);
        CardsManager.Instance.GetCardByIndex(p_cardIndex).cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.TrySetParent(transform, false);
    }

    [ServerRpc]
    public void SetItemCardParentServerRpc(ItemType p_itemType)
    {
        SetItemCardParentClientRpc(p_itemType);
    }

    [ClientRpc]
    public void SetItemCardParentClientRpc(ItemType p_itemType)
    {
        Item l_item = CardsManager.Instance.GetItemNetworkObject(p_itemType, PlayerIndex);
        l_item.cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.TrySetParent(transform, false);

        m_handBehavior.AddItemOnHand(l_item);
    }


    [ClientRpc]
    void RemoveCardFromGameClientRpc()
    {
        CardsManager.Instance.RemoveCardFromGame();
    }

    [ClientRpc]
    void AnimCardClientRpc(int p_playerType, int p_targetIndex, NetworkObjectReference p_cardNetworkObjectReference)
    {
        if (IsOwner && p_playerType != PlayerIndex)
        {
            if (p_cardNetworkObjectReference.TryGet(out NetworkObject p_cardNetworkObject))
            {
                p_cardNetworkObject.GetComponent<CardBehavior>().AnimateToPlace(CardsManager.Instance.GetCardTargetByIndex(p_targetIndex, p_playerType), CardAnimType.PLAY);
            }
        }
    }

    #region Items
    [Button]
    public void UseScissors()
    {
        CardsManager.Instance.UseScissorServerRpc((Player)PlayerIndex);

    }
    #endregion

}
