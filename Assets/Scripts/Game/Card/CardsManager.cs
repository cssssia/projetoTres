using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//cannot set a network object to a parent that was dinamically spawned -> limitation
public class CardsManager : NetworkBehaviour
{
    public static CardsManager Instance;

    public event EventHandler OnAddCardToMyHand;
    public event EventHandler OnRemoveCardFromMyHand;
    public event EventHandler OnAddItemCardToMyHand;

    [Header("Cards Lists")]
    public List<int> CardsOnGameList;
    public List<int> DeckOnGameList;
    public List<Card> UsableDeckList;
    public List<Item> UsableItemsList;

    [Header("Cards")]
    [SerializeField] private NetworkObject m_deckParent;
    [SerializeField] private CardsScriptableObject m_cardsSO;
    [SerializeField] private ItemCardScriptableObject m_itemsSO;

    [Header("Items")]
    public List<bool> PlayersHaveItem = new List<bool> { false, false };
    public bool BothPlayersHaveItem => PlayersHaveItem[0] && PlayersHaveItem[1];

    [Header("Targets")]
    [SerializeField] private List<CardTarget> m_targetsTranform;
    [SerializeField] private List<CardTransform> m_targets;
    [Space, SerializeField] private Transform m_itemTargetTranform;
    public CardTransform ItemTarget { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        DeckOnGameList = new List<int>();

        if (IsServer)
        {
            SelectUsableCardsInSO();
            SpawnItems();
        }
        SetCardTargets();
        SetItemTargets();

        RoundManager.Instance.OnItemUsed += OnItemUsed;
    }

    void SetCardTargets()
    {
        for (int i = 0; i < m_targetsTranform.Count; i++)
        {
            m_targets.Add(new(m_targetsTranform[i].transform.position, m_targetsTranform[i].transform.rotation.eulerAngles, m_targetsTranform[i].transform.localScale));
        }
    }

    void SetItemTargets()
    {
        ItemTarget = new(m_itemTargetTranform.position, m_itemTargetTranform.rotation.eulerAngles, m_itemTargetTranform.localScale);
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

    void SpawnItems()
    {
        for (int i = 0; i < m_itemsSO.initialItems.Length; i++)
        {
            SpawnItemCardServerRpc(m_itemsSO.initialItems[i]);
        }
    }

    [ServerRpc]
    public void SpawnNewPlayCardsServerRpc()
    {
        Debug.Log("[GAME] Spawn Cards");

        for (int i = 0; i < 3 * MultiplayerManager.MAX_PLAYER_AMOUNT; i++)
        {
            int l_rand = UnityEngine.Random.Range(0, DeckOnGameList.Count);
            SetPlayerUsableDeckClientRpc(DeckOnGameList[l_rand], (Player)(i % 2));
            bool l_lastCartd = (Player)(i % 2) == Player.HOST ? i == 4 : i == 5;
            DealOneCardClientRpc(DeckOnGameList[l_rand], l_lastCartd);
        }
    }
    [ClientRpc]
    void DealOneCardClientRpc(int p_cardIndex, bool p_isLastCard)
    {
        DeckOnGameList.Remove(p_cardIndex);
        CardsOnGameList.Add(p_cardIndex);
        OnAddCardToMyHand?.Invoke((p_cardIndex, p_isLastCard), EventArgs.Empty);
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
        l_cardNetworkObject.TrySetParent(m_deckParent, true);

        Card l_usableCard = new(m_cardsSO.deck[p_cardIndexSO].name, m_cardsSO.deck[p_cardIndexSO].value, p_cardIndexSO, p_cardNetworkObjectReference);

        UsableDeckList.Add(l_usableCard);
        DeckOnGameList.Add(UsableDeckList.Count - 1);
    }

    [ServerRpc]
    public void DealItemsToPlayersServerRpc()
    {
        DealItemsToPlayersClientRpc();
    }

    [ClientRpc]
    public void DealItemsToPlayersClientRpc()
    {
        ItemType l_randomItem = ItemType.SCISSORS;//randomize it

        if (!PlayersHaveItem[0])
        {
            PlayersHaveItem[0] = true;
            OnAddItemCardToMyHand.Invoke((l_randomItem, 0), EventArgs.Empty);
        }
        if (!PlayersHaveItem[1])
        {
            PlayersHaveItem[1] = true;
            OnAddItemCardToMyHand.Invoke((l_randomItem, 1), EventArgs.Empty);
        }
    }

    [ServerRpc]
    void SpawnItemCardServerRpc(ItemType p_itemType)
    {
        GameObject l_newCard = Instantiate(m_itemsSO.Prefab, m_itemsSO.InitialPosition, Quaternion.Euler(m_itemsSO.InitialRotation));
        NetworkObject l_cardNetworkObject = l_newCard.GetComponent<NetworkObject>();
        l_cardNetworkObject.Spawn(true);

        RenameItemCardServerRpc(l_cardNetworkObject, p_itemType);
    }

    [ServerRpc]
    void RenameItemCardServerRpc(NetworkObjectReference p_cardNetworkObjectReference, ItemType p_itemType) //for a pattern, maybe ? (the tutorial guy does it)
    {
        RenameItemCardClientRpc(p_cardNetworkObjectReference, p_itemType);
    }

    [ClientRpc]
    void RenameItemCardClientRpc(NetworkObjectReference p_cardNetworkObjectReference, ItemType p_itemType)
    {
        p_cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.name = m_itemsSO.GetItemConfig(p_itemType).objectName;
        l_cardNetworkObject.GetComponent<MeshRenderer>().material = m_itemsSO.GetItemConfig(p_itemType).material;
        l_cardNetworkObject.TrySetParent(m_deckParent, true);

        Item l_usableCard = new(p_itemType, p_cardNetworkObjectReference, Player.DEFAULT, UsableItemsList != null && UsableItemsList.Count > 0
                                                                            ? UsableItemsList.Count : 0);

        if (UsableItemsList == null) UsableItemsList = new();
        UsableItemsList.Add(l_usableCard);
    }

    private void OnItemUsed(object p_itemIndex, EventArgs p_args)
    {
        int l_itemID = (int)p_itemIndex;
        PlayersHaveItem[(int)UsableItemsList[l_itemID].playerID] = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseItemServerRpc(int p_itemIndex)
    {
        switch (UsableItemsList[p_itemIndex].Type)
        {
            case ItemType.NONE:
                break;
            case ItemType.SCISSORS:
                UseScissorServerRpc(UsableItemsList[p_itemIndex].playerID);
                break;
        }

    }

    public List<int> l_cardsToRemove = new List<int>();
    [ServerRpc(RequireOwnership = false)]
    public void UseScissorServerRpc(Player p_playerId)
    {
        Card l_card;

        if (l_cardsToRemove == null) l_cardsToRemove = new();
        else l_cardsToRemove.Clear();

        for (int i = 0; i < CardsOnGameList.Count; i++)
        {
            l_card = GetCardByIndex(CardsOnGameList[i]);
            if (l_card.cardPlayer == p_playerId && !l_card.playedCard) l_cardsToRemove.Add(CardsOnGameList[i]);
        }

        int l_quantityOfCardsRemoved = l_cardsToRemove.Count;

        for (int i = CardsOnGameList.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < l_cardsToRemove.Count; j++)
            {
                if (CardsOnGameList[i] == l_cardsToRemove[j])
                {
                    int l_cardID = CardsOnGameList[i];
                    RemoveCardClientRpc(l_cardID);
                    SoftResetCardServerRpc(l_cardID);

                    break;
                }
            }
        }

        for (int i = 0; i < l_quantityOfCardsRemoved; i++)
        {
            int l_rand = UnityEngine.Random.Range(0, DeckOnGameList.Count);

            SetPlayerUsableDeckClientRpc(DeckOnGameList[l_rand], p_playerId);
            DealOneCardClientRpc(DeckOnGameList[l_rand], i + 1 == l_quantityOfCardsRemoved);
        }

        SetHasItemClientRpc((int)p_playerId, false);
    }

    [ClientRpc]
    private void SetHasItemClientRpc(int p_playerIndex, bool p_hasItem)
    {
        PlayersHaveItem[p_playerIndex] = p_hasItem;
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

                break;
            }
        }
    }

    [ClientRpc]
    public void SetPlayerUsableDeckClientRpc(int p_cardIndex, Player p_player)
    {
        GetCardByIndex(p_cardIndex).cardPlayer = p_player;
    }

    [ClientRpc]
    public void SetPlayedCardUsableDeckClientRpc(int p_cardIndex, bool p_playedCard)
    {
        GetCardByIndex(p_cardIndex).playedCard = p_playedCard;
    }

    [ClientRpc]
    public void SetDeckAsCardParentClientRpc(int p_cardIndex)
    {
        GetCardByIndex(p_cardIndex).cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.transform.SetPositionAndRotation(m_cardsSO.InitialPosition, Quaternion.Euler(m_cardsSO.InitialRotation));
        l_cardNetworkObject.TrySetParent(m_deckParent, true);
    }

    public void RemoveCardsFromGame()
    {
        for (int i = CardsOnGameList.Count - 1; i >= 0; i--)
        {
            ResetCardServerRpc(CardsOnGameList[i]);
        }
    }

    [ClientRpc]
    public void AddCardToDeckClientRpc(int p_cardIndex)
    {
        CardsOnGameList.Remove(p_cardIndex);
        DeckOnGameList.Add(p_cardIndex);
    }

    [ServerRpc]
    public void ResetCardServerRpc(int p_cardIndex)
    {
        SetPlayerUsableDeckClientRpc(p_cardIndex, Player.DEFAULT);
        SetPlayedCardUsableDeckClientRpc(p_cardIndex, false);
        SetDeckAsCardParentClientRpc(p_cardIndex);

        AddCardToDeckClientRpc(p_cardIndex);

        if (m_softResetedCards.Count > 0)
        {
            for (int i = 0; i < m_softResetedCards.Count; i++)
            {
                AddCardToDeckClientRpc(m_softResetedCards[i]);
            }

            m_softResetedCards.Clear();
        }

    }

    List<int> m_softResetedCards = new List<int>();
    [ServerRpc]
    public void SoftResetCardServerRpc(int p_cardIndex)
    {
        SetPlayerUsableDeckClientRpc(p_cardIndex, Player.DEFAULT);
        SetPlayedCardUsableDeckClientRpc(p_cardIndex, false);
        SetDeckAsCardParentClientRpc(p_cardIndex);

        m_softResetedCards.Add(p_cardIndex);
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

    public Item GetItemByIndex(int p_index)
    {
        return UsableItemsList[p_index];
    }

    public Item GetItemNetworkObject(ItemType p_itemType, Player p_playerdId)
    {
        //bool l_found = false;

        for (int i = 0; i < UsableItemsList.Count; i++)
        {
            if (UsableItemsList[i].Type == p_itemType && /*!UsableItemsList[i].isOnGame*/UsableItemsList[i].playerID == Player.DEFAULT)
            {
                UsableItemsList[i].playerID = p_playerdId;
                return UsableItemsList[i];
                //break;
            }
        }

        return null;
        //if (!l_found) SpawnItemCardServerRpc(p_itemType);
        //StartCoroutine(WaitItemSpawn(p_itemType, p_onSpawn));
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetItemServerRpc(int p_itemID)
    {
        ResetItemClientRpc(p_itemID);
    }

    [ClientRpc]
    public void ResetItemClientRpc(int p_itemID)
    {
        UsableItemsList[p_itemID].ResetItem();
        UsableItemsList[p_itemID].cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
        l_cardNetworkObject.transform.SetPositionAndRotation(m_itemsSO.InitialPosition, Quaternion.Euler(m_itemsSO.InitialRotation));
        l_cardNetworkObject.TrySetParent(m_deckParent, true);
    }

    //IEnumerator WaitItemSpawn(ItemType p_itemType, Action<Item> p_onSpawn)
    //{
    //    bool l_found = false;
    //    while (!l_found)
    //    {
    //        for (int i = 0; i < UsableItemsList.Count; i++)
    //        {
    //            if (UsableItemsList[i].Type == p_itemType && !UsableItemsList[i].isOnGame)
    //            {
    //                p_onSpawn?.Invoke(UsableItemsList[i]);
    //                l_found = true;
    //                break;
    //            }
    //        }

    //        if (l_found) break;
    //        else yield return null;
    //    }
    //}
}
