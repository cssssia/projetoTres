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
    [SerializeField] private List<Vector3> m_spawnRotationList;
    [SerializeField] private CardTarget[] m_cardTargetTransform;

    [SerializeField] private CardsScriptableObject m_cardsSO;
    [SerializeField] private CardsOnHandBehavior m_handBehavior;

    [SerializeField] private List<UsableCard> m_myHand;
    [SerializeField] private List<NetworkObject> m_myHandNetworkObjects;

    [Header("Game Info")]
    [SerializeField] private GameManager.GameState currentGameState;

    void Start()
    {
        if (!IsOwner)
            return;
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
        }

        transform.SetPositionAndRotation(m_spawnPositionList[PlayerIndex], Quaternion.Euler(0, IsHostPlayer ? 0 : 180, 0));
        if (IsOwner) CameraController.Instance.SetCamera(PlayerIndex);

        CardsManager.Instance.OnAddCardToMyHand += CardsManager_OnAddCardToMyHand;
        RoundManager.Instance.OnRoundWon += TurnManager_OnRoundWon;
        //TurnManager.Instance.OnCardPlayed += TurnManager_OnCardPlayed;

        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        if (IsOwner)
        {
            GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
            GameInput.Instance.OnMoveMouse += GameInput_OnMoveMouse;
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

    // private void TurnManager_OnCardPlayed(object p_playerType, EventArgs e)
    // {
    //     if (IsOwner && GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId) == (int)p_playerType)

    // }

    private void TurnManager_OnRoundWon(object p_playerWonId, EventArgs e)
    {
        RemoveCardFromGameClientRpc();
        
        if (IsOwner)
        {
            for (int i = 0; i < m_myHandNetworkObjects.Count; i++)
                RemoveCardVisualFromMyHandServerRpc(m_myHandNetworkObjects[i]);

            m_myHand.Clear();
            m_myHandNetworkObjects.Clear();
            m_handBehavior.ResetTargetIndex();
        }

        if (IsOwner && PlayerIndex == (int)p_playerWonId)
            Debug.Log("[GAME] You Won!");


        if (IsServer)
            RoundManager.Instance.RoundHasStarted.Value = false;
    }


    private void CardsManager_OnAddCardToMyHand(object p_indexes, EventArgs e)
    {
        Indexes l_indexes = (Indexes)p_indexes;

        if (IsOwner && l_indexes.cardIndexDeal % 2 == PlayerIndex)
        {
            UsableCard l_usableCard = new();

            l_usableCard.Card = m_cardsSO.deck[l_indexes.cardIndexSO];
            l_usableCard.OriginalSOIndex = l_indexes.cardIndexSO;

            l_indexes.networkObjectReference.TryGet(out NetworkObject l_networkObject);

            m_myHand.Add(l_usableCard);
            m_myHandNetworkObjects.Add(l_networkObject);

            Debug.Log(m_myHand.Count);

            SetCardParentServerRpc(l_networkObject, m_myHand.Count == 3);
        }
    }

    [ServerRpc]
    public void SetCardParentServerRpc(NetworkObjectReference p_cardNetworkObjectReference, bool p_finishedHandCards)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        SetCardParentClientRpc(p_cardNetworkObjectReference, p_finishedHandCards);
    }

    [ClientRpc]
    public void SetCardParentClientRpc(NetworkObjectReference p_cardNetworkObjectReference, bool p_finishedHandCards)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        if (IsOwner)
        {
            m_handBehavior.AddCardOnHand(l_cardNetworkObject, p_finishedHandCards);
        }
        l_cardNetworkObject.TrySetParent(transform, false);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong p_clientId)
    {
        if (p_clientId == OwnerClientId)
        {
            Debug.Log("[INFO] Owner Disconnected");
            // destroy network stuff
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
        }
    }
    private void GameInput_OnClickUpMouse(object p_sender, System.EventArgs e)
    {
        m_handBehavior.CheckClickUp(canPlay, (go) => ThrowCard(go));
    }

    private void CheckClickOnObjects(GameObject p_gameObject)
    {
        //atualmente, s� est� checando cartas, mas aqui podemos chegar itens tambem

        bool l_find = m_handBehavior.CheckClickObject(p_gameObject);
    }
    private void CheckHoverOnObject(GameObject p_gameObject)
    {
        bool l_find = m_handBehavior.CheckHoverObject(p_gameObject);

        //if (l_find) Debug.Log("[INFO] Hover");
    }

    void Update()
    {
        if (!IsOwner)
            return;
    }

    private void ThrowCard(GameObject gameObject)
    {
        Debug.Log("play card: " + gameObject.name);
        if (gameObject.CompareTag("Card"))
        {
            for (int i = 0; i < m_myHand.Count; i++)
            {
                if (m_myHand[i].Card.name == gameObject.name)
                {

                    if (!RoundManager.Instance.RoundHasStarted.Value)
                        RoundManager.Instance.StartMatchServerRpc(IsHost ? Player.HOST : Player.CLIENT);

                    int l_soIndex = m_myHand[i].OriginalSOIndex;
                    m_myHand.RemoveAt(i);
                    AnimCardThrowFromMyHandClientRpc(PlayerIndex, m_handBehavior.CurrentTargetIndex, m_myHandNetworkObjects[i]);
                    m_myHandNetworkObjects.RemoveAt(i);
                    //RemoveCardVisualFromMyHandServerRpc(gameObject.GetComponent<NetworkObject>());

                    RoundManager.Instance.PlayCardServerRpc(l_soIndex, IsHost ? Player.HOST : Player.CLIENT);
                }
            }
        }
    }

    [ClientRpc]
    void RemoveCardFromGameClientRpc()
    {
        CardsManager.Instance.RemoveCardFromGame();
    }

    [ServerRpc]
    void RemoveCardVisualFromMyHandServerRpc(NetworkObjectReference p_cardNetworkObjectReference)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.Despawn();
    }

    [ClientRpc]
    void AnimCardThrowFromMyHandClientRpc(int p_playerID, int p_targetIndex, NetworkObjectReference p_cardNetworkObjectReference)
    {
        if(p_playerID != PlayerIndex)
        {
            if (p_cardNetworkObjectReference.TryGet(out NetworkObject p_cardNetworkObject))
            {
                p_cardNetworkObject.GetComponent<CardBehavior>().AnimateToPlace(CardsManager.Instance.GetCardTargetByIndex(p_targetIndex), CardAnimType.PLAY);
            }
            
        }
    }

    public void AddToMyHand(int p_cardIndexSO)
    {
        UsableCard l_usableCard = new();

        l_usableCard.Card = m_cardsSO.deck[p_cardIndexSO];
        l_usableCard.OriginalSOIndex = p_cardIndexSO;

        m_myHand.Add(l_usableCard);
    }

    [ServerRpc]
    public void RemoveAllCardsFromHandServerRpc()
    {
        RemoveAllCardsFromHandClientRpc();
    }


    [ClientRpc]
    public void RemoveAllCardsFromHandClientRpc()
    {

        if (IsOwner)
        {
            int l_myHandCount = m_myHand.Count - 1;

            for (int i = l_myHandCount; i >= 0; i--)
            {
                NetworkObject l_removedNetworkObject = m_myHandNetworkObjects[i];
                m_myHand.RemoveAt(i);
                m_myHandNetworkObjects.RemoveAt(i);
                RemoveCardVisualFromMyHandServerRpc(l_removedNetworkObject);
            }

            m_handBehavior.RemoveAllCardsFromHandBehavior();
            RemoveAllCardsServerRpc(1);
        }

    }

    int aux = 0;
    [ServerRpc(RequireOwnership = false)]
    public void RemoveAllCardsServerRpc(int p_aux)
    {
        aux += p_aux;

        if (aux == 2 && IsServer)
        {
            Debug.Log("aaa");
            CardsManager.Instance.SpawnNewPlayCardsServerRpc();
            aux = 0;
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

}
