using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

//cannot set a network object to a parent that was dinamically spawned -> limitation

public class CardsManager : NetworkBehaviour
{
    public static CardsManager Instance;

    [SerializeField] private GameObject m_deckParent;
    [SerializeField] private List<Vector3> m_cardSpawnPositionList;
    [SerializeField] private List<Vector3> m_cardSpawnRotationList;
    [SerializeField] private CardsScriptableObject m_cardsSO;

    [SerializeField] private List<CardsScriptableObject.Card> m_usableDeckList;
    [SerializeField] private HashSet<ulong>.Enumerator m_observers;

    //NetworkVariable<float> testVariable = new NetworkVariable<float>(0f); //leave other parameters blank to everyone read, but only server write
    //network variables fire an event whenever the variable changes (as it is a network variable, listen to it on spawn, not start not awake)

    // public override void OnNetworkSpawn()
    // {
    //     testVariable.OnValueChanged += TestVariable_OnValueChanged;
    // }

    // private void TestVariable_OnValueChanged(float previousValue, float newValue)
    // {
    //        testVariable.Value //to access the variable
    // }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    void Update()
    {
        if (!IsServer)
            return;

        // m_dealDeckTimer -= Time.deltaTime;
        // if (m_dealDeckTimer <= 0f) //timer to have time to connect host and client (only for tests)
        // {
        //     m_dealDeckTimer = m_dealDeckTimerMax;

        //     if (!m_cardsWereSpawned)
        //     {
        //         SpawnNewPlayCardsServerRpc();
        //     }

        // }

    }

    void SelectUsableCardsInSO()
    {
        for (int i = 0; i < m_cardsSO.deck.Count; i++)
        {
            if (m_cardsSO.deck[i].value == 0)
                continue;

            m_usableDeckList.Add(m_cardsSO.deck[i]);
        }
    }

    [ServerRpc] //[ServerRpc(RequireOwnership = false)] clients can call the function, but it runs on the server
    public void SpawnNewPlayCardsServerRpc() //can only instantiate prefabs on server AND only destroy on server
    {

        SelectUsableCardsInSO();
        Shuffle(m_usableDeckList);

        for (int i = 0; i < 3 * GameMultiplayerManager.MAX_PLAYER_AMOUNT; i++)
        {
            GameObject l_newCard = Instantiate(m_cardsSO.prefab, m_cardSpawnPositionList[i], Quaternion.Euler(m_cardSpawnRotationList[i]));
            NetworkObject l_cardNetworkObject = l_newCard.GetComponent<NetworkObject>();
            l_cardNetworkObject.Spawn(true);
            RenameCardServerRpc(l_cardNetworkObject, i);
        }

    }

    [ServerRpc]
    void RenameCardServerRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_index) //for a pattern, maybe ? (the tutorial guy does it)
    {
        RenameCardClientRpc(p_cardNetworkObjectReference, p_index);
    }

    // int i;
    [ClientRpc]
    void RenameCardClientRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_index)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.name = m_usableDeckList[p_index].name;
        l_cardNetworkObject.GetComponent<MeshRenderer>().material = m_usableDeckList[p_index].material;
        //l_cardNetworkObject.GetComponent<SpriteRenderer>().sprite = m_usableDeckList[p_index].sprite;
        //l_cardNetworkObject.GetComponent<SpriteRenderer>().sortingOrder = p_index / GameMultiplayerManager.MAX_PLAYER_AMOUNT;
        //l_cardNetworkObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = p_index / GameMultiplayerManager.MAX_PLAYER_AMOUNT;
        l_cardNetworkObject.TrySetParent(m_deckParent, false); //false to ignore WorldPositionStays and to work as we are used to (also do it on the client to sync position)
        m_observers = NetworkObject.GetObservers();

        if (PlayerController.LocalInstance.OwnerClientId == Convert.ToUInt64(p_index % 2))
            PlayerController.LocalInstance.AddToMyHand(m_usableDeckList[p_index]);

        // if (p_index % 2 == 0)
        //     GameMultiplayerManager.Instance.GetPlayerControllerFromId(0).AddToMyHand(m_usableDeckList[p_index]);
        // PlayerController.LocalInstance.AddToMyHand(m_usableDeckList[p_index]);
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 0)
        {
            int k = random.Next(n);
            n--;
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

}
