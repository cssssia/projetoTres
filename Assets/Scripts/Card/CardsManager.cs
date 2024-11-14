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
    public List<int> CardsOnGameList;
    public List<int> DeckOnGameList;
    public List<Card> UsableDeckList;
    [SerializeField] private NetworkObject m_deckParent;
    [SerializeField] private CardsScriptableObject m_cardsSO;
    [SerializeField] private ItemCardScriptableObject m_itemsSO;

    public event EventHandler OnAddCardToMyHand;
    public event EventHandler OnRemoveCardFromMyHand;

    [Header("Targets")]
    [SerializeField] private List<CardTarget> m_targetsTranform;
    [SerializeField] private List<CardTransform> m_targets;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        DeckOnGameList = new List<int>();

        if (IsServer) SelectUsableCardsInSO();
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
        for (int i = 0; i < m_cardsSO.deck.Count; i++)
        {
            if (m_cardsSO.deck[i].value == 0)
                continue;

            SpawnCardServerRpc(i);
        }
    }

    [ServerRpc]
    public void SpawnNewPlayCardsServerRpc()
    {
        Debug.Log("[GAME] Spawn Cards");

        CardsOnGameList = new List<int>();

        Shuffle(DeckOnGameList);

        for (int i = 0; i < 3 * GameMultiplayerManager.MAX_PLAYER_AMOUNT; i++)
        {
            SetPlayerUsableDeckClientRpc(DeckOnGameList[i], (Player)(i % 2));
            DealOneCard(DeckOnGameList[i]);
        }

    }

    [ClientRpc]
    public void SetPlayerUsableDeckClientRpc(int p_cardIndex, Player p_player)
    {
        GetCardByIndex(p_cardIndex).cardPlayer = p_player;
    }

    void DealOneCard(int p_cardIndex)
    {
        DeckOnGameList.Remove(p_cardIndex);

        DealOneCardClientRpc(p_cardIndex);
    }

    [ClientRpc]
    void DealOneCardClientRpc(int p_cardIndex)
    {
        CardsOnGameList.Add(p_cardIndex);
        OnAddCardToMyHand?.Invoke(p_cardIndex, EventArgs.Empty);
    }

    [ServerRpc]
    void SpawnCardServerRpc(int p_indexSO)
    {
        GameObject l_newCard = Instantiate(m_cardsSO.Prefab, m_cardsSO.InitialPosition, Quaternion.Euler(m_cardsSO.InitialRotation));
        NetworkObject l_cardNetworkObject = l_newCard.GetComponent<NetworkObject>();
        l_cardNetworkObject.Spawn(true);

        RenameCardServerRpc(l_cardNetworkObject, p_indexSO);
    }

    [ServerRpc]
    void RenameCardServerRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_cardIndexSO) //for a pattern, maybe ? (the tutorial guy does it)
    {
        RenameCardClientRpc(p_cardNetworkObjectReference, p_cardIndexSO);
    }

    [ClientRpc]
    void RenameCardClientRpc(NetworkObjectReference p_cardNetworkObjectReference, int p_cardIndexSO)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.name = m_cardsSO.deck[p_cardIndexSO].name;
        l_cardNetworkObject.GetComponent<MeshRenderer>().material = m_cardsSO.deck[p_cardIndexSO].material;
        l_cardNetworkObject.TrySetParent(m_deckParent, false);

        Card l_usableCard = new(m_cardsSO.deck[p_cardIndexSO].name, m_cardsSO.deck[p_cardIndexSO].value, p_cardIndexSO, p_cardNetworkObjectReference);

        UsableDeckList.Add(l_usableCard);
        DeckOnGameList.Add(UsableDeckList.Count - 1);
    }

    private void OnCardPlayed(object p_cardIndex, EventArgs p_args)
    {
        for (int i = 0; i < CardsOnGameList.Count; i++)
        {
            if (CardsOnGameList[i] == (int)p_cardIndex)
            {
                SetPlayedCardUsableDeckClientRpc(CardsOnGameList[i], true);
                return;
            }
        }
    }

    [ClientRpc]
    void SetPlayedCardUsableDeckClientRpc(int p_cardIndex, bool p_playedCard)
    {
        GetCardByIndex(p_cardIndex).playedCard = p_playedCard;
    }


    [ServerRpc(RequireOwnership = false)]
    public void UseScissorServerRpc(Player p_playerId)
    {
        List<int> l_cardsToRemove = new List<int>();
        Card l_card;

        for (int i = 0; i < CardsOnGameList.Count; i++)
        {
            l_card = GetCardByIndex(CardsOnGameList[i]);
            if (l_card.cardPlayer == p_playerId && !l_card.playedCard) l_cardsToRemove.Add(CardsOnGameList[i]);
        }

        int l_quantityOfCardsRemoved = l_cardsToRemove.Count;

        for (int j = 0; j < CardsOnGameList.Count; j++)
        {
            for (int i = 0; i < l_cardsToRemove.Count; i++)
            {
                if (CardsOnGameList[j] == l_cardsToRemove[i])
                {
                    RemoveCardClientRpc(CardsOnGameList[j]);
                    break;
                }
            }
        }

        for (int i = 0; i < l_quantityOfCardsRemoved; i++)
        {
            int l_rand = UnityEngine.Random.Range(0, DeckOnGameList.Count);

            UsableDeckList[DeckOnGameList[l_rand]].cardPlayer = p_playerId;
            DealOneCard(DeckOnGameList[l_rand]);
            AddCardClientRpc(DeckOnGameList[l_rand]);

            DeckOnGameList.RemoveAt(l_rand);
        }

    }

    [ClientRpc]
    private void RemoveCardClientRpc(int p_cardIndex)
    {
        for (int i = 0; i < CardsOnGameList.Count; i++)
        {
            if (CardsOnGameList[i] == p_cardIndex)
            {
                OnRemoveCardFromMyHand?.Invoke(p_cardIndex, EventArgs.Empty);
                CardsOnGameList.RemoveAt(i);

                SetPlayerUsableDeckClientRpc(p_cardIndex, Player.DEFAULT);
                SetPlayedCardUsableDeckClientRpc(p_cardIndex, false);

                break;
            }
        }
    }

    [ClientRpc]
    private void AddCardClientRpc(int p_cardIndex)
    {
        CardsOnGameList.Add(p_cardIndex);
    }


    [ServerRpc]
    void RemoveCardVisualGameServerRpc(int p_cardIndex)
    {
        //GetCardByIndex(p_cardIndex).cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        //l_cardNetworkObject.Despawn();
    }

    public void RemoveCardFromGame()
    {
        if (!IsServer)
        {
            CardsOnGameList.Clear();
            return;
        }

        for (int i = CardsOnGameList.Count - 1; i >= 0; i--)
        {
            int l_removeCard = CardsOnGameList[i];
            CardsOnGameList.Remove(l_removeCard);
            RemoveCardVisualGameServerRpc(l_removeCard);
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

    public Card GetCardByIndex(int index)
    {
        return UsableDeckList[index];
    }
}
