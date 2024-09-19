using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// C:\Users\Usuario\AppData\LocalLow\DefaultCompany\projetoTres > Player.log

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set;}

    [SerializeField] private List<Vector3> m_spawnPositionList;
    [SerializeField] private CardsScriptableObject m_cardsSO;
    [SerializeField] private int m_index;

    [SerializeField] private List<UsableCard> m_myHand;
    [SerializeField] private List<NetworkObject> m_myHandNetworkObjects;
	public event EventHandler OnMatchEnd;

    void Start()
    {
        if (!IsOwner)
            return;
    }

    public override void OnNetworkSpawn() //research more the difference of this and awake
    {

        if (IsOwner)
            LocalInstance = this;

        transform.position = m_spawnPositionList[GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)]; //do this on camera later

        if (IsOwner)
            CameraController.Instance.SetCamera(GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        CardsManager.Instance.OnAddCardToMyHand += CardsManager_OnAddCardToMyHand;
        RoundManager.Instance.OnRoundWon += TurnManager_OnRoundWon;
        //TurnManager.Instance.OnCardPlayed += TurnManager_OnCardPlayed;

        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        if (IsOwner)
    		GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

    }

    // private void TurnManager_OnCardPlayed(object p_playerType, EventArgs e)
    // {
    //     if (IsOwner && GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId) == (int)p_playerType)
            
    // }

    private void TurnManager_OnRoundWon(object p_playerWonId, EventArgs e)
    {
        if (IsOwner)
        {
            for (int i = 0; i < m_myHandNetworkObjects.Count; i++)
                RemoveCardVisualFromMyHandServerRpc(m_myHandNetworkObjects[i]);

            m_myHand.Clear();
            m_myHandNetworkObjects.Clear();
        }

        if (IsOwner && GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId) == (int)p_playerWonId)
            Debug.Log("You Won!");

        if (IsServer)
            RoundManager.Instance.RoundHasStarted.Value = false;

    }

    private void CardsManager_OnAddCardToMyHand(object p_indexes, EventArgs e)
    {
        Indexes l_indexes = (Indexes)p_indexes;

        if (IsOwner && l_indexes.cardIndexDeal % 2 == GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId))
        {
            UsableCard l_usableCard = new();

            l_usableCard.Card = m_cardsSO.deck[l_indexes.cardIndexSO];
            l_usableCard.OriginalSOIndex = l_indexes.cardIndexSO;

            l_indexes.networkObjectReference.TryGet(out NetworkObject l_networkObject);

            m_myHand.Add(l_usableCard);
            m_myHandNetworkObjects.Add(l_networkObject);
        }

    }

    private void NetworkManager_OnClientDisconnectCallback(ulong p_clientId)
    {
        if (p_clientId == OwnerClientId)
        {
            Debug.Log("owner disconnected");
            // destroy network stuff
        }
    }

    private void GameInput_OnInteractAction(object p_sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsMyTurn(IsHost))
        {
            RaycastHit l_raycastHit;

            Ray l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(l_ray, out l_raycastHit, 10f))
            {
                if (l_raycastHit.transform != null)
                {
                    //Our custom method.
                    CurrentClickedGameObject(l_raycastHit.transform.gameObject);
                }
            }
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;
    }

    private void CurrentClickedGameObject(GameObject gameObject)
    {
        if(gameObject.CompareTag("Card"))
        {
            for (int i = 0; i < m_myHand.Count; i++)
            {
                if (m_myHand[i].Card.name == gameObject.name)
                {

                    if (!RoundManager.Instance.RoundHasStarted.Value)
                        RoundManager.Instance.StartMatchServerRpc(IsHost ? Player.HOST : Player.CLIENT);

                    RoundManager.Instance.PlayCardServerRpc(m_myHand[i].OriginalSOIndex, IsHost ? Player.HOST : Player.CLIENT);

                    m_myHand.RemoveAt(i);
                    m_myHandNetworkObjects.RemoveAt(i);
                    RemoveCardVisualFromMyHandServerRpc(gameObject.GetComponent<NetworkObject>());

                }
            }
        }
    }

    [ServerRpc]
    void RemoveCardVisualFromMyHandServerRpc(NetworkObjectReference p_cardNetworkObjectReference)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.Despawn();
    }

    public void AddToMyHand(int p_cardIndexSO)
    {
        UsableCard l_usableCard = new();

        l_usableCard.Card = m_cardsSO.deck[p_cardIndexSO];
        l_usableCard.OriginalSOIndex = p_cardIndexSO;

        m_myHand.Add(l_usableCard);
    }

}
