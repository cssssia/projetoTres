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
    public event EventHandler OnRemoveCardFromMyHand;

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

        RoundManager.Instance.OnCardPlayed += OnCardPlayed;
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
            SpawnOneCard(m_usableDeckList[i], i % 2);
        }

    }

    /// <summary>
    /// Has to be called by a ServerRpc
    /// </summary>
    void SpawnOneCard(Card p_card, int p_playerIndex)
    {
        GameObject l_newCard = Instantiate(m_cardsSO.prefab);
        NetworkObject l_cardNetworkObject = l_newCard.GetComponent<NetworkObject>();
        l_cardNetworkObject.Spawn(true);
        p_card.cardNetworkObject = l_cardNetworkObject;
        m_deckOnGameList.Remove(p_card);
        RenameCardServerRpc(l_cardNetworkObject, p_card.cardIndexSO, p_playerIndex);
    }

    [ServerRpc]
    void RenameCardServerRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_cardIndexSO, int p_playerIndex) //for a pattern, maybe ? (the tutorial guy does it)
    {
        RenameCardClientRpc(p_cardNetworkObjectReference, p_cardIndexSO, p_playerIndex);
    }

    [ClientRpc]
    void RenameCardClientRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_cardIndexSO, int p_playerIndex)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.name = m_cardsSO.deck[p_cardIndexSO].name;
        l_cardNetworkObject.GetComponent<MeshRenderer>().material = m_cardsSO.deck[p_cardIndexSO].material;

        Card l_card = new Card();

        l_card.cardName = l_cardNetworkObject.name;
        l_card.cardValue = m_cardsSO.deck[p_cardIndexSO].value;
        l_card.cardIndexSO = p_cardIndexSO;
        l_card.cardPlayer = p_playerIndex;
        l_card.cardNetworkObject = l_cardNetworkObject;
        cardsOnGameList.Add(l_card);
        //l_cardNetworkObject.TrySetParent(m_deckParent, false); //false to ignore WorldPositionStays and to work as we are used to (also do it on the client to sync position)
        OnAddCardToMyHand?.Invoke(l_card, EventArgs.Empty);
    }

    private void OnCardPlayed(object p_sender, EventArgs p_args)
    {
        CustomSender l_customSender = (CustomSender)p_sender;
        for (int i = 0; i < cardsOnGameList.Count; i++)
        {
            if (cardsOnGameList[i].cardIndexSO == l_customSender.cardIndex)
            {
                cardsOnGameList[i].playedCard = true;
                return;
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void UseScissorServerRpc(int p_playerID)
    {
        List<int> l_cardsToRemove = cardsOnGameList.Where(c => c.cardPlayer == p_playerID && !c.playedCard).Select(s => s.cardIndexSO).ToList();
        int l_quantityOfCardsRemoved = l_cardsToRemove.Count;

        Card l_card = new Card();
        for (int j = 0; j < cardsOnGameList.Count; j++)
        {
            for (int i = 0; i < l_cardsToRemove.Count; i++)
            {
                if (cardsOnGameList[j].cardIndexSO == l_cardsToRemove[i])
                {
                    l_card = cardsOnGameList[j];
                    RemoveCardVisualGameServerRpc(l_card.cardNetworkObject);
                    RemoveCardClientRpc(l_card.cardIndexSO);
                    break;
                }
            }
        }

        for (int i = 0; i < l_quantityOfCardsRemoved; i++)
        {
            int l_rand = UnityEngine.Random.Range(0, m_deckOnGameList.Count);
            //cardsOnGameList.Add(l_cardsToAdd[i]);

            SpawnOneCard(m_deckOnGameList[l_rand], p_playerID);
            AddCardClientRpc(m_deckOnGameList[l_rand].cardIndexSO);

            m_deckOnGameList.RemoveAt(l_rand);
        }

    }

    [ClientRpc]
    private void RemoveCardClientRpc(int p_cardID)
    {
        //if (IsHost) return;

        for (int i = 0; i < cardsOnGameList.Count; i++)
        {
            if (cardsOnGameList[i].cardIndexSO == p_cardID)
            {
                OnRemoveCardFromMyHand?.Invoke(cardsOnGameList[i], EventArgs.Empty);
                cardsOnGameList.RemoveAt(i);

                break;
            }
        }
    }

    [ClientRpc]
    private void AddCardClientRpc(int p_cardID)
    {
        //if (IsHost) return;

        for (int i = 0; i < m_usableDeckList.Count; i++)
        {
            if (m_usableDeckList[i].cardIndexSO == p_cardID)
            {
                cardsOnGameList.Add(m_usableDeckList[i]);

                break;
            }
        }
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
