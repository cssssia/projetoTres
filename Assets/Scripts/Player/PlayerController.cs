using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using NaughtyAttributes;

// C:\Users\Usuario\AppData\LocalLow\DefaultCompany\projetoTres > Player.log

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set; }

    [SerializeField] private List<Vector3> m_spawnPositionList;
    [SerializeField] private CardTarget[] m_cardTargetTransform;
    [SerializeField] private CardsOnHandBehavior m_handBehavior;
    [SerializeField] private List<Card> m_myHand;

    [SerializeField] private BetBehavior m_betBehavior;


    [Header("Game Info")]
    [SerializeField] private GameManager.GameState currentGameState;

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

    private bool IsHostPlayer
    {
        get { return PlayerIndex == 0; }
    }
    private bool IsClientPlayer
    {
        get { return PlayerIndex == 1; }
    }

    private int PlayerIndex
    {
        get { return GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId); }
    }

    private bool canPlay;

    public override void OnNetworkSpawn() //research more the difference of this and awake
    {
        if (IsOwner)
        {
            LocalInstance = this;
            m_handBehavior.OnPlayerSpawned();
            m_betBehavior.OnPlayerSpawned(PlayerIndex);
        }

        transform.SetPositionAndRotation(m_spawnPositionList[PlayerIndex], Quaternion.Euler(0, IsHostPlayer ? 0 : 180, 0));
        if (IsOwner) CameraController.Instance.SetCamera(PlayerIndex);

        CardsManager.Instance.OnAddCardToMyHand += CardsManager_OnAddCardToMyHand;
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

        GameManager.Instance.OnStateChanged += OnGameStateChanged;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong p_clientId)
    {
        if (p_clientId == OwnerClientId)
        {
            Debug.Log("[INFO] Owner Disconnected");
            // destroy network stuff
        }
    }

    private void CardsManager_OnAddCardToMyHand(object p_card, EventArgs e)
    {
        Card l_card = (Card)p_card;

        if (IsOwner && l_card.cardPlayer == PlayerIndex)
        {
            m_myHand.Add(l_card);
            SetCardParentServerRpc(l_card.cardNetworkObject, m_myHand.Count - 1, m_myHand.Count == 3);
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
        }

        RemoveCardFromGameClientRpc();

        if (IsOwner && PlayerIndex == (int)p_playerWonId)
            Debug.Log("[GAME] You Won!");

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
        }
    }

    private void GameInput_OnClickUpMouse(object p_sender, System.EventArgs e)
    {
        m_handBehavior.CheckClickUp(canPlay, (go, id) => StartAnim(go, id), (go) => ThrowCard(go));
        m_betBehavior.CheckClickUp(canPlay, (go) => IncreaseBet(go));
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
                if (m_myHand[i].cardName == gameObject.name)
                {

                    if (!RoundManager.Instance.RoundHasStarted.Value)
                        RoundManager.Instance.StartRoundServerRpc(IsHost ? Player.HOST : Player.CLIENT);

                    int l_soIndex = m_myHand[i].cardIndexSO;
                    NetworkObject l_cardNetworkObject = m_myHand[i].cardNetworkObject;

                    m_myHand.RemoveAt(i);
                    RoundManager.Instance.PlayCardServerRpc(l_soIndex, IsHost ? Player.HOST : Player.CLIENT, m_handBehavior.CurrentTargetIndex, l_cardNetworkObject);
                }
            }
        }
    }

    private void IncreaseBet(GameObject gameObject)
    {
        if (gameObject.CompareTag("Bet"))
        {
            Debug.Log("Bet " + gameObject.name);
        }
    }

    public void OnGameStateChanged(object p_sender, EventArgs p_eventArgs)
    {
        if (!IsOwner) return;

        currentGameState = ((GameManager)p_sender).gameState.Value;
        canPlay = (currentGameState == GameManager.GameState.HostPlayerTurn && IsHostPlayer)
                                            || (currentGameState == GameManager.GameState.ClientPlayerTurn && IsClientPlayer);

        if (canPlay) Debug.Log("pode jogar");
        else Debug.Log("não pode jogar");
    }

    [ServerRpc]
    public void SetCardParentServerRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_indexOnHand, bool p_finishedHandCards)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        SetCardParentClientRpc(p_cardNetworkObjectReference, p_indexOnHand, p_finishedHandCards);
    }

    [ClientRpc]
    public void SetCardParentClientRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_indexOnHand, bool p_finishedHandCards)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        if (IsOwner)
        {
            m_handBehavior.AddCardOnHand(l_cardNetworkObject, m_myHand[p_indexOnHand], p_finishedHandCards);
        }
        l_cardNetworkObject.TrySetParent(transform, false);
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
        CardsManager.Instance.UseScissorServerRpc(PlayerIndex);

    }
    #endregion

}
