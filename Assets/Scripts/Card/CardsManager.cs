using System;
using System.Collections;
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
    public List<Item> UsableItemsList;

    [SerializeField] private NetworkObject m_deckParent;
    [SerializeField] private CardsScriptableObject m_cardsSO;
    [SerializeField] private ItemCardScriptableObject m_itemsSO;

    public event EventHandler OnAddCardToMyHand;
    public event EventHandler OnRemoveCardFromMyHand;

    public event EventHandler OnAddItemCardToMyHand;

    [Header("Items")]
    public List<bool> PlayersHaveItem = new List<bool> { false, false };
    public bool BothPlayersHaveItem => PlayersHaveItem[0] && PlayersHaveItem[1];

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
        CardsOnGameList = new List<int>();

        if (IsServer)
        {
            SelectUsableCardsInSO();
            SpawnItems();
        }
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

        //Shuffle(DeckOnGameList);

        for (int i = 0; i < 3 * GameMultiplayerManager.MAX_PLAYER_AMOUNT; i++)
        {
            int l_rand = UnityEngine.Random.Range(0, DeckOnGameList.Count);
            SetPlayerUsableDeckClientRpc(DeckOnGameList[l_rand], (Player)(i % 2));
            DealOneCardClientRpc(DeckOnGameList[l_rand]);
        }
    }

    [ClientRpc]
    public void SetPlayerUsableDeckClientRpc(int p_cardIndex, Player p_player)
    {
        GetCardByIndex(p_cardIndex).cardPlayer = p_player;
    }

    [ClientRpc]
    void DealOneCardClientRpc(int p_cardIndex)
    {
        DeckOnGameList.Remove(p_cardIndex);
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

    [ServerRpc]
    public void DealItemsToPlayersServerRpc()
    {
        DealItemsToPlayersClientRpc();
    }

    [ClientRpc]
    public void DealItemsToPlayersClientRpc()
    {
        ItemType l_randomItem = ItemType.SCISSORS;//randomize it

        if (!PlayersHaveItem[0]) OnAddItemCardToMyHand.Invoke((l_randomItem, 0), EventArgs.Empty);
        if (!PlayersHaveItem[1]) OnAddItemCardToMyHand.Invoke((l_randomItem, 1), EventArgs.Empty);
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
        l_cardNetworkObject.TrySetParent(m_deckParent, false);

        Item l_usableCard = new(p_itemType, p_cardNetworkObjectReference, -1);

        if (UsableItemsList == null) UsableItemsList = new();
        UsableItemsList.Add(l_usableCard);
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
            DealOneCardClientRpc(DeckOnGameList[l_rand]);
            AddCardClientRpc(DeckOnGameList[l_rand]);

            DeckOnGameList.RemoveAt(l_rand);
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
        GetCardByIndex(p_cardIndex).cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);

        l_cardNetworkObject.transform.SetPositionAndRotation(m_cardsSO.InitialPosition, Quaternion.Euler(m_cardsSO.InitialRotation));
        //l_cardNetworkObject.Despawn();
    }

    public void RemoveCardFromGame()
    {
        Debug.Log("RemoveCardFromGame");

        for (int i = CardsOnGameList.Count - 1; i >= 0; i--)
        {
            int l_removeCard = CardsOnGameList[i];
            CardsOnGameList.Remove(l_removeCard);

            GetCardByIndex(l_removeCard).cardNetworkObjectReference.TryGet(out NetworkObject l_cardNetworkObject);
            l_cardNetworkObject.transform.SetPositionAndRotation(m_cardsSO.InitialPosition, Quaternion.Euler(m_cardsSO.InitialRotation));
            l_cardNetworkObject.TrySetParent(m_deckParent, false);

            ResetCard(l_removeCard);

            DeckOnGameList.Add(l_removeCard);
        }

    }

    void ResetCard(int p_cardIndex)
    {
        Debug.Log("ResetCard " + PlayerController.LocalInstance.PlayerIndex);

        Card l_card = GetCardByIndex(p_cardIndex);
        l_card.playedCard = false;
        l_card.cardPlayer = Player.DEFAULT;
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

    public Item GetItemNetworkObject(ItemType p_itemType, int p_playerdId)
    {
        bool l_found = false;

        for (int i = 0; i < UsableItemsList.Count; i++)
        {
            if (UsableItemsList[i].Type == p_itemType && /*!UsableItemsList[i].isOnGame*/UsableItemsList[i].playerId == -1)
            {
                UsableItemsList[i].playerId = p_playerdId;
                return UsableItemsList[i];
                //break;
            }
        }

        return null;
        //if (!l_found) SpawnItemCardServerRpc(p_itemType);
        //StartCoroutine(WaitItemSpawn(p_itemType, p_onSpawn));
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
