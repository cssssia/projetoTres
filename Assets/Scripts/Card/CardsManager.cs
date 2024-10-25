using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//cannot set a network object to a parent that was dinamically spawned -> limitation
public class CardsManager : NetworkBehaviour
{
    public static CardsManager Instance;

    [SerializeField] private GameObject m_deckParent;
    [SerializeField] private List<Vector3> m_cardSpawnPositionList;
    [SerializeField] private List<Vector3> m_cardSpawnRotationList;
    [SerializeField] private CardsScriptableObject m_cardsSO;

    [SerializeField] private List<UsableCard> m_usableCardList;
    [SerializeField] private HashSet<ulong>.Enumerator m_observers;
	public event EventHandler OnAddCardToMyHand;

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
    }

    void SelectUsableCardsInSO()
    {
        for (int i = 0; i < m_cardsSO.deck.Count; i++)
        {
            if (m_cardsSO.deck[i].value == 0)
                continue;

            UsableCard l_usableCard = new();

            l_usableCard.Card = m_cardsSO.deck[i];
            l_usableCard.OriginalSOIndex = i;

            m_usableCardList.Add(l_usableCard);
        }
    }

    [ServerRpc] //[ServerRpc(RequireOwnership = false)] clients can call the function, but it runs on the server
    public void SpawnNewPlayCardsServerRpc() //can only instantiate prefabs on server AND only destroy on server
    {

        Debug.Log("Spawn Cards");

        SelectUsableCardsInSO();
        Shuffle(m_usableCardList);

        for (int i = 0; i < 3 * GameMultiplayerManager.MAX_PLAYER_AMOUNT; i++)
        {
            GameObject l_newCard = Instantiate(m_cardsSO.prefab, m_cardSpawnPositionList[i], Quaternion.Euler(m_cardSpawnRotationList[i]));
            NetworkObject l_cardNetworkObject = l_newCard.GetComponent<NetworkObject>();
            l_cardNetworkObject.Spawn(true);
            RenameCardServerRpc(l_cardNetworkObject, m_usableCardList[i].OriginalSOIndex, i);
        }

    }

    [ServerRpc]
    void RenameCardServerRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_cardIndexSO, int p_cardIndex) //for a pattern, maybe ? (the tutorial guy does it)
    {
        RenameCardClientRpc(p_cardNetworkObjectReference, p_cardIndexSO, p_cardIndex);
    }

    [ClientRpc]
    void RenameCardClientRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_cardIndexSO, int p_cardIndex)
    {
        Indexes l_indexes = new();

        l_indexes.cardIndexSO = p_cardIndexSO;
        l_indexes.cardIndexDeal = p_cardIndex;
        l_indexes.networkObjectReference = p_cardNetworkObjectReference;

        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.name = m_cardsSO.deck[p_cardIndexSO].name;
        l_cardNetworkObject.GetComponent<MeshRenderer>().material = m_cardsSO.deck[p_cardIndexSO].material;
        //l_cardNetworkObject.TrySetParent(m_deckParent, false); //false to ignore WorldPositionStays and to work as we are used to (also do it on the client to sync position)
        OnAddCardToMyHand?.Invoke(l_indexes, EventArgs.Empty);
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
