using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using NaughtyAttributes;
using System.Collections;

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
    [SerializeField] private int m_itemOnHand;

    [Header("Game Info")]
    [SerializeField] private GameManager.GameState currentGameState;
    [SerializeField] private GameManager.BetState currentBetState;

    [Space]
    [SerializeField] private HandController m_handController;
    private Queue<Action> m_actionsQueue;


    void Start()
    {
        if (!IsOwner)
            return;
    }

    //[Header("Debug buttons")]
    //public bool debug_useScissors;
    //private void Update()
    //{
    //    if (!IsOwner) return;
    //    if (debug_useScissors)
    //    {
    //        UseScissors();
    //        debug_useScissors = false;
    //    }
    //}

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
        get { return MultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId); }
    }

    public bool CanPlay { get; private set; }
    private bool canBet;

    public override void OnNetworkSpawn() //research more the difference of this and awake
    {
        Debug.Log($"{(Player)PlayerIndex} at OnNetworkSpawn PLAYER - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            RoundManager.Instance.OnStartPlayingCard += TurnManager_OnStartPlayingCard; // TO-DO move this up and we will not need to use client rpc
        }

        if (IsOwner)
        {
            LocalInstance = this;

            GameInput.Instance.OnMoveMouse += GameInput_OnMoveMouse;
            GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
            GameInput.Instance.OnStopInteractAction += GameInput_OnClickUpMouse;

            GameManager.Instance.IsAnyAnimationPlaying.OnValueChanged += OnAnyAnimationPlayingChanged;

            CameraController.Instance.SetCamera(PlayerIndex);

            m_betBehavior.OnPlayerSpawned(PlayerIndex);
            m_deckBehavior.OnPlayerSpawned();

            m_actionsQueue = new();

            m_cardTargetTransform = FindObjectsByType<CardTarget>(FindObjectsSortMode.None);
            for (int i = 0; i < m_cardTargetTransform.Length; i++)
            {
                if (m_cardTargetTransform[i].clientID == PlayerIndex)
                    m_handBehavior.AddTarget(m_cardTargetTransform[i].transform, m_cardTargetTransform[i].targetIndex);
            }

            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnBetStateChanged += OnBetStateChanged;
        }

        m_handBehavior.OnPlayerSpawned(this);

        transform.SetPositionAndRotation(m_spawnData.spawnPosition[PlayerIndex], Quaternion.Euler(m_spawnData.spawnRotation[PlayerIndex]));

        CardsManager.Instance.OnAddCardToMyHand += CardsManager_OnAddCardToMyHand;
        CardsManager.Instance.OnRemoveCardFromMyHand += CardsManager_OnRemoveCardFromMyHand;
        CardsManager.Instance.OnAddItemCardToMyHand += CardsManager_OnAddItemCardToMyHand;

        RoundManager.Instance.OnRoundWon += TurnManager_OnRoundWon;

        if (IsHostPlayer) name = "Player_0";
        else if (IsClient) name = "Player_1";

    }

    private void NetworkManager_OnClientDisconnectCallback(ulong p_clientId)
    {
        if (p_clientId == OwnerClientId)
        {
            Debug.Log("[INFO] Owner Disconnected");
            // destroy network stuff

            for (int i = 0; i < m_myHand.Count; i++)
            {
                CardsManager.Instance.ResetCardServerRpc(m_myHand[i]);
            }
        }
    }

    private void CardsManager_OnAddCardToMyHand(object p_cardSended, EventArgs e)
    {
        int p_cardIndex = (((int, bool))p_cardSended).Item1;
        bool p_lastCard = (((int, bool))p_cardSended).Item2;

        if (CardsManager.Instance.GetCardByIndex(p_cardIndex).cardPlayer == (Player)PlayerIndex)
        {
            m_myHand.Add(p_cardIndex);
            m_handBehavior.AddCardOnHand(p_cardIndex, p_lastCard);
            CardsManager.Instance.GetCardByIndex(p_cardIndex).cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
            l_cardNetworkObject.TrySetParent(transform, true);
        }
    }
    private void CardsManager_OnAddItemCardToMyHand(object p_itemTypeAndPlayer, EventArgs e)
    {
        ItemType p_itemType = (((ItemType, int))p_itemTypeAndPlayer).Item1;
        int p_playerId = (((ItemType, int))p_itemTypeAndPlayer).Item2;

        if (p_playerId == PlayerIndex)
        {
            Item l_item = CardsManager.Instance.GetItemNetworkObject(p_itemType, (Player)PlayerIndex);
            l_item.cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
            l_cardNetworkObject.TrySetParent(transform, true);
            m_itemOnHand = l_item.itemID;

            m_handBehavior.AddItemOnHand(l_item);
        }
    }

    private void CardsManager_OnRemoveCardFromMyHand(object p_cardIndex, EventArgs e)
    {
        int l_cardID = (int)p_cardIndex;
        m_myHand.Remove(l_cardID);
        m_handBehavior.RemoveCardFromHand(l_cardID);
    }

    private void TurnManager_OnStartPlayingCard(object p_customSender, EventArgs e)
    {
        Debug.Log($"{(Player)PlayerIndex} at TurnManager_OnStartPlayingCard - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        CustomSender l_customSender = (((CustomSender, bool))p_customSender).Item1;
        bool l_isItem = (((CustomSender, bool))p_customSender).Item2;
        AnimCardClientRpc(l_customSender.playerType, l_customSender.targetIndex, l_isItem, l_customSender.cardNO);
    }

    private void TurnManager_OnRoundWon(object p_playerWonId, EventArgs e)
    {
        Debug.Log($"{(Player)PlayerIndex} at TurnManager_OnRoundWon - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        if (IsServer)
            Debug.Log($"[GAME] {(Player)p_playerWonId} Won!");

        if (IsOwner)
        {
            if (PlayerIndex == (int)p_playerWonId)
                Debug.Log("[GAME] You Won!");

            if (IsServer)
            {
                CardsManager.Instance.RemoveCardsFromGame();
            }
        }

        m_myHand.Clear();
        m_handBehavior.ResetCardsOnHandBehavior();

        if (!IsOwner && IsServer)
        {
            RoundManager.Instance.RoundHasStarted.Value = false;
        }
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
        m_handBehavior.CheckClickUp(CanPlay,
                                    (go, isItem, id) => StartAnim(go, isItem, id),
                                    (go) => ThrowCard(go), (go) => UseItemCard(go));
        if (CanPlay || RoundManager.Instance.BetHasStarted.Value) m_betBehavior.CheckClickUp(canBet, (go, increase) => IncreaseBet(go, increase));
        if (RoundManager.Instance.BetHasStarted.Value && canBet) m_deckBehavior.CheckClickUp((go) => GiveUp(go));
    }

    private void CheckClickOnObjects(GameObject p_gameObject)
    {
        m_handBehavior.CheckClickObject(p_gameObject);
        m_betBehavior.CheckClickObject(p_gameObject);
        m_deckBehavior.CheckClickObject(p_gameObject);
    }

    private void CheckHoverOnObject(GameObject p_gameObject)
    {
        m_handBehavior.CheckHoverObject(p_gameObject);
    }

    private void StartAnim(GameObject gameObject, bool p_isItem, int p_cardIndex)
    {
        RoundManager.Instance.OnStartAnimServerRpc(PlayerIndex, p_isItem, m_handBehavior.CurrentTargetIndex,
                                                    p_cardIndex, gameObject.GetComponent<NetworkObject>());
    }

    private void ThrowCard(GameObject p_gameObject)
    {
        GameManager.Instance.SetPlayerAnimatingServerRpc(false);

        AddFunctionToQueue(() =>
        {
            Debug.Log("[GAME] Will Throw Card " + p_gameObject.name);
            if (p_gameObject.CompareTag("Card"))
            {
                for (int i = 0; i < m_myHand.Count; i++)
                {
                    if (CardsManager.Instance.GetCardByIndex(m_myHand[i]).cardName == p_gameObject.name)
                    {
                        if (!RoundManager.Instance.RoundHasStarted.Value)
                            RoundManager.Instance.StartRoundServerRpc(IsHost ? Player.HOST : Player.CLIENT);

                        int l_cardPlayedIndex = m_myHand[i];
                        m_myHand.Remove(l_cardPlayedIndex);
                        RoundManager.Instance.PlayCardServerRpc(l_cardPlayedIndex, IsHost ? Player.HOST : Player.CLIENT, m_handBehavior.CurrentTargetIndex);
                    }
                }
            }
        });
    }

    //private void ThrowCardOnQueue()

    private void UseItemCard(GameObject p_gameObject)
    {
        StartCoroutine(IUseItemCard(m_itemOnHand, () =>
        {
            GameManager.Instance.SetPlayerAnimatingServerRpc(false);
            AddFunctionToQueue(() =>
            {
                Debug.Log("[GAME] Use Item " + p_gameObject.name);
                RoundManager.Instance.PlayItemCardServerRpc(m_itemOnHand);
                CardsManager.Instance.ResetItemServerRpc(m_itemOnHand);
                m_itemOnHand = -1;
            });
        }));
    }

    IEnumerator IUseItemCard(int p_itemIndex, Action p_action)
    {
        ItemType l_type = CardsManager.Instance.GetItemByIndex(p_itemIndex).Type;
        bool l_waiting = true;
        switch (l_type)
        {
            case ItemType.NONE:
                break;
            case ItemType.SCISSORS:
                m_handBehavior.AnimCardCut((go) => { l_waiting = false; });
                m_handController.OnCutCards += CutCards;
                break;
        }
        while (l_waiting) yield return null;
        
        l_waiting = true;

        m_handController.HandItem(l_type, () => { l_waiting = false; });

        while (l_waiting) yield return null;

        p_action.Invoke();
    }

    private void CutCards()
    {
        m_handController.OnCutCards -= CutCards;
        m_handBehavior.AnimCardDestroy();
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
        CanPlay = ((currentGameState == GameManager.GameState.HostTurn && IsHostPlayer)
                                            || (currentGameState == GameManager.GameState.ClientTurn && IsClientPlayer))
                                            && !RoundManager.Instance.BetHasStarted.Value;

        if (CanPlay) Debug.Log("pode jogar");
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

    CardBehavior l_tempCard;
    [ClientRpc]
    void AnimCardClientRpc(int p_playerType, int p_targetIndex, bool p_isItem, NetworkObjectReference p_cardNetworkObjectReference)
    {
        if (p_cardNetworkObjectReference.TryGet(out NetworkObject p_cardNetworkObject))
            l_tempCard = p_cardNetworkObject.GetComponent<CardBehavior>();

        if (IsOwner && p_playerType != PlayerIndex)
        {
            if (l_tempCard != null)
            {
                GameManager.Instance.SetPlayerAnimatingServerRpc(true);
                if (!p_isItem)
                    l_tempCard.AnimateToPlace(CardsManager.Instance.GetCardTargetByIndex(p_targetIndex, p_playerType),
                                              CardAnimType.PLAY,
                                              (go) =>
                                                      {
                                                          GameManager.Instance.SetPlayerAnimatingServerRpc(false);
                                                      });
                else l_tempCard.AnimateToPlace(CardsManager.Instance.ItemTarget, CardAnimType.PLAY,
                        (go) =>
                        {
                            GameManager.Instance.SetPlayerAnimatingServerRpc(false);
                        });
            }
        }

        if (!IsOwner && p_playerType == PlayerIndex)
        {
            if (l_tempCard != null)
            {
                bool l_removed = m_handBehavior.RemoveCard(l_tempCard);

                if (l_removed)
                {
                    m_handBehavior.ReorderCards();
                }
            }
        }
    }

    public void OnAnyAnimationPlayingChanged(bool p_lastValue, bool p_newValue)
    {
        if (!p_newValue) PlayActionQueue();
    }

    public void AddFunctionToQueue(Action p_action)
    {
        m_actionsQueue.Enqueue(p_action);

        if (!GameManager.Instance.IsAnyAnimationPlaying.Value) PlayActionQueue();
        else print("não pode tocar imediatamente");
    }

    void PlayActionQueue()
    {
        while (m_actionsQueue.Count > 0)
        {
            m_actionsQueue.Dequeue().Invoke();
        }
    }
}
