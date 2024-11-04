using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

//cannot set a network object to a parent that was dinamically spawned -> limitation
public class CardsManager : NetworkBehaviour
{
    public static CardsManager Instance;

    [Header("Cards")]
    public List<Card> cardsOnGameList;
    [SerializeField] private List<Card> m_deckOnGameList;
    [SerializeField] private List<Card> m_usableDeckList;
    [SerializeField] private CardsScriptableObject m_cardsSO;

	public event EventHandler OnAddCardToMyHand;

    [Header("Targets")]
    [SerializeField] private List<CardTarget> m_targetsTranform;
    [SerializeField] private List<CardTransform> m_targets;

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SelectUsableCardsInSO();
        SetCardTargets();
    }

    void SetCardTargets()
    {
        for (int i = 0; i < m_targetsTranform.Count; i++)
        {
            m_targets.Add(new(m_targetsTranform[i].transform.position, m_targetsTranform[i].transform.rotation.eulerAngles, m_targetsTranform[i].transform.localScale));
        }
    }

    void Update()
    {
        if (!IsServer)
            return;
    }

    void SelectUsableCardsInSO()
    {
        m_usableDeckList = new List<Card>();

        for (int i = 0; i < m_cardsSO.deck.Count; i++)
        {
            if (m_cardsSO.deck[i].value == 0)
                continue;

            Card l_usableCard = new Card();

            l_usableCard.cardName = m_cardsSO.deck[i].name;
            l_usableCard.cardValue = m_cardsSO.deck[i].value;
            l_usableCard.cardIndexSO = i;
            l_usableCard.cardPlayer = 3;

            m_usableDeckList.Add(l_usableCard);
        }
    }

    [ServerRpc] //[ServerRpc(RequireOwnership = false)] clients can call the function, but it runs on the server
    public void SpawnNewPlayCardsServerRpc() //can only instantiate prefabs on server AND only destroy on server
    {
        Debug.Log("[GAME] Spawn Cards");

        cardsOnGameList = new List<Card>();
        m_deckOnGameList = new List<Card>();

        Shuffle(m_usableDeckList);
        m_deckOnGameList = m_usableDeckList.ToList();

        for (int i = 0; i < 3 * GameMultiplayerManager.MAX_PLAYER_AMOUNT; i++)
        {
            GameObject l_newCard = Instantiate(m_cardsSO.prefab);
            NetworkObject l_cardNetworkObject = l_newCard.GetComponent<NetworkObject>();
            l_cardNetworkObject.Spawn(true);
            m_usableDeckList[i].cardNetworkObject = l_cardNetworkObject;
            m_deckOnGameList.Remove(m_usableDeckList[i]);
            RenameCardServerRpc(l_cardNetworkObject, m_usableDeckList[i].cardIndexSO, i);
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
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.name = m_cardsSO.deck[p_cardIndexSO].name;
        l_cardNetworkObject.GetComponent<MeshRenderer>().material = m_cardsSO.deck[p_cardIndexSO].material;

        Card l_card = new Card();

        l_card.cardName = l_cardNetworkObject.name;
        l_card.cardValue = m_cardsSO.deck[p_cardIndexSO].value;
        l_card.cardIndexSO = p_cardIndexSO;
        l_card.cardPlayer = p_cardIndex % 2;
        l_card.cardNetworkObject = l_cardNetworkObject;
        cardsOnGameList.Add(l_card);
        //l_cardNetworkObject.TrySetParent(m_deckParent, false); //false to ignore WorldPositionStays and to work as we are used to (also do it on the client to sync position)
        OnAddCardToMyHand?.Invoke(l_card, EventArgs.Empty);
    }

    [ServerRpc]
    void RemoveCardVisualGameServerRpc(NetworkObjectReference p_cardNetworkObjectReference)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.Despawn();
    }

    public void RemoveCardFromGame()
    {
        if (!IsServer)
        {
            cardsOnGameList.Clear();
            return;
        }

        for (int i = cardsOnGameList.Count - 1; i >= 0; i--)
        {
            Card l_removeCard = cardsOnGameList[i];
            cardsOnGameList.Remove(l_removeCard);
            RemoveCardVisualGameServerRpc(l_removeCard.cardNetworkObject);
        }

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

    public CardTransform GetCardTargetByIndex(int p_index, int p_playerType)
    {
        for (int i = 0; i < m_targets.Count; i++)
        {
            if (m_targetsTranform[i].targetIndex == p_index && m_targetsTranform[i].clientID == p_playerType) return m_targets[i];
        }

        return null;
    }
}
