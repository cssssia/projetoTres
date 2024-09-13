using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// C:\Users\Usuario\AppData\LocalLow\DefaultCompany\projetoTres > Player.log

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set;}

    [SerializeField] private List<Vector3> m_spawnPositionList;
    [SerializeField] private List<CardsScriptableObject.Card> m_myHand;
    [SerializeField] private CardsScriptableObject m_cardsSO;
    [SerializeField] private int m_index;

    private List<UsableDeck> usableDeckList;
    [SerializeField] private List<UsableDeck> m_usableDeckList;

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

        if (IsHost && IsOwner)
            

        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        if (IsOwner)
    		GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

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

    void Update()
    {
        if (!IsOwner)
            return;
    }

    private void CurrentClickedGameObject(GameObject gameObject)
    {
        if(gameObject.CompareTag("Card"))
        {

Debug.Log("Card");
                Debug.Log(m_myHand.Count);

            for (int i = 0; i < m_myHand.Count; i++)
            {
                Debug.Log("meu for");
                if (m_myHand[i].name == gameObject.name)
                {
                    
        Debug.Log(m_myHand[i].name);
                    if (TurnManager.Instance.CurrentMatch.WhoStartedMatch.Equals(Player.DEFAULT))
                    {
                        TurnManager.Instance.StartMatchServerRpc(IsHost ? Player.HOST : Player.CLIENT);
                    } else
                    {
                        Debug.Log(TurnManager.Instance.CurrentMatch.WhoStartedMatch);
                    }


                    
                    TurnManager.Instance.PlayCardServerRpc(m_usableDeckList[i].OriginalSOIndex, IsHost ? Player.HOST : Player.CLIENT);
                }
        }
    }
    }
    public void AddToMyHand(int p_cardIndexSO)
    {
        

        m_myHand.Add(m_cardsSO.deck[p_cardIndexSO]);
        UsableDeck l_usableDeck = new();

        l_usableDeck.UsableCard = m_cardsSO.deck[p_cardIndexSO];
        l_usableDeck.OriginalSOIndex = p_cardIndexSO;

         m_usableDeckList.Add(l_usableDeck);
    }

    // [ServerRpc]
    // public void AddToMyHandServerRpc(int p_index)
    // {
    //     Debug.Log(IsHost);
    //     Debug.Log(IsOwner);
    //     Debug.Log(IsServer);
    //     Debug.Log(p_index);
    // }

    // [ServerRpc (RequireOwnership = false)]
    // public void AddToMyHandOwnServerRpc(int p_index)
    // {
    //     Debug.Log(IsHost);
    //     Debug.Log(IsOwner);
    //     Debug.Log(IsServer);
    //     Debug.Log(p_index);
    // }

    [ClientRpc]
    public void AddToMyHandClientRpc(int p_cardIndexSO, int p_cardIndex)
    {

        Debug.Log(GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        if (GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId) == p_cardIndex % 2){
                    m_myHand.Add(m_cardsSO.deck[p_cardIndexSO]);
        UsableDeck l_usableDeck = new();

        l_usableDeck.UsableCard = m_cardsSO.deck[p_cardIndexSO];
        l_usableDeck.OriginalSOIndex = p_cardIndexSO;

         m_usableDeckList.Add(l_usableDeck);
        }
            
        // else if  (GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId) == 1)
        //     AddToMyHand(p_cardIndexSO);
    }
}
