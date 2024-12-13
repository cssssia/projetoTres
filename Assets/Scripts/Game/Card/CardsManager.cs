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

    public event EventHandler OnAddCardToMyHand;
    public event EventHandler OnRemoveCardFromMyHand;
    public event EventHandler OnAddItemCardToMyHand;

    public event EventHandler ReturnEyes;

    [Header("Cards Lists")]
    public List<int> CardsOnGameList;
    public List<Card> UsableDeckList;
    public List<Item> UsableItemsList;

    [Header("Cards")]
    [SerializeField] private GameObject m_deckParent;
    [SerializeField] private GameObject m_itemDeckParent;
    [SerializeField] private CardsScriptableObject m_cardsSO;
    [SerializeField] private ItemCardScriptableObject m_itemsSO;

    [Header("Items")]
    public List<bool> PlayersHaveItem = new List<bool> { false, false };
    public bool BothPlayersHaveItem => PlayersHaveItem[0] && PlayersHaveItem[1];
    public ItemType spawnThisItem = ItemType.NONE;

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
        Debug.Log($"{(Player)OwnerClientId} at OnNetworkSpawnCardsManager - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        SetCardTargets();
        SetItemTargets();

        RoundManager.Instance.OnItemUsed += OnItemUsed;
        RoundManager.Instance.RetractCards += RetractCards;
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

    public CardTransform GetCardTargetByIndex(int p_index, int p_playerType)
    {
        for (int i = 0; i < m_targets.Count; i++)
        {
            if (m_targetsTranform[i].targetIndex == p_index && m_targetsTranform[i].clientID == p_playerType) return m_targets[i];
        }

        return null;
    }

    [ServerRpc]
    public void DealCardsServerRpc()
    {
        Debug.Log("[GAME] Deal Cards");

        for (int i = 0; i < 3 * MultiplayerManager.MAX_PLAYER_AMOUNT; i++)
        {
            int l_rand;

            do
            {
                l_rand = UnityEngine.Random.Range(0, UsableDeckList.Count);
            }
            while (GetCardByIndex(l_rand).playerId != Player.DEFAULT);

            bool l_lastCartd = (Player)(i % 2) == Player.HOST ? i == 4 : i == 5;
            DealCardsClientRpc(l_rand, (Player)(i % 2), l_lastCartd);
        }
    }

    List<Item> l_tempItemList;
    [ServerRpc]
    public void DealItemsServerRpc()
    {
        for (int i = 0; i < MultiplayerManager.MAX_PLAYER_AMOUNT; i++)
        {
            int l_randomItem;

            if (spawnThisItem == ItemType.SCISSORS)
            {
                l_tempItemList = UsableItemsList.Where(item => item.playerId == Player.DEFAULT && item.type is ItemType.SCISSORS).ToList();
                l_randomItem = l_tempItemList[UnityEngine.Random.Range(0, l_tempItemList.Count)].id;
            }
            else if (spawnThisItem == ItemType.STAKE)
            {
                l_tempItemList = UsableItemsList.Where(item => item.playerId == Player.DEFAULT && item.type is ItemType.STAKE).ToList();
                l_randomItem = l_tempItemList[UnityEngine.Random.Range(0, l_tempItemList.Count)].id;
            }
            else
            {
                l_tempItemList = UsableItemsList.Where(item => item.playerId == Player.DEFAULT).ToList();
                l_randomItem = l_tempItemList[UnityEngine.Random.Range(0, l_tempItemList.Count)].id;
            }

            DealItemsClientRpc(l_randomItem, i);
        }
    }

    [ClientRpc]
    public void DealCardsClientRpc(int p_cardIndex, Player p_playerId, bool p_isLastCard = false)
    {
        GetCardByIndex(p_cardIndex).playerId = p_playerId;

        if (p_playerId != Player.DEFAULT)
        {
            CardsOnGameList.Add(p_cardIndex);
            SetCardOnGame(p_cardIndex, true);
            OnAddCardToMyHand?.Invoke((p_cardIndex, p_isLastCard), EventArgs.Empty);
        }
        else
        {
            CardsOnGameList.Remove(p_cardIndex);
            SetCardOnGame(p_cardIndex, false);
            SetDeckAsCardParent(p_cardIndex);
            GetCardByIndex(p_cardIndex).GetCardBehavior.ResetToDeck();
            OnRemoveCardFromMyHand?.Invoke(p_cardIndex, EventArgs.Empty);
        }
    }

    [ClientRpc]
    public void DealItemsClientRpc(int p_itemIndex, int p_playerIndex)
    {
        if (!PlayersHaveItem[p_playerIndex])
        {
            PlayersHaveItem[p_playerIndex] = true;
            GetItemByIndex(p_itemIndex).playerId = (Player)p_playerIndex;
            OnAddItemCardToMyHand.Invoke((p_itemIndex, p_playerIndex), EventArgs.Empty);
        }
    }

    [ServerRpc]
    public void HighlightCardServerRpc(int p_cardIndex)
    {
        if (p_cardIndex == -1) return;
        HighlightCardClientRpc(p_cardIndex);
    }

    [ClientRpc]
    void HighlightCardClientRpc(int p_cardIndex)
    {
        GetCardByIndex(p_cardIndex).GetCardBehavior.AnimCardWinHighlight();
    }

    [ClientRpc]
    public void SetCardOnGameClientRpc(int p_cardIndex, bool p_playedCard)
    {
        SetCardOnGame(p_cardIndex, p_playedCard);
    }

    void SetCardOnGame(int p_cardIndex, bool p_playedCard)
    {
        GetCardByIndex(p_cardIndex).isOnGame = p_playedCard;
    }

    void SetDeckAsCardParent(int p_cardIndex)
    {
        GameObject l_cardGameObject = GetCardByIndex(p_cardIndex).gameObject;
        l_cardGameObject.transform.SetPositionAndRotation(m_cardsSO.InitialPosition, Quaternion.Euler(m_cardsSO.InitialRotation));
        l_cardGameObject.transform.SetParent(m_deckParent.transform, true);
    }

    [ClientRpc]
    private void SetHasItemClientRpc(int p_playerIndex, bool p_hasItem)
    {
        PlayersHaveItem[p_playerIndex] = p_hasItem;
    }

    public Card GetCardByIndex(int p_index)
    {
        return UsableDeckList[p_index];
    }

    public Item GetItemByIndex(int p_index)
    {
        return UsableItemsList[p_index];
    }

    public void RemoveCardsFromGame()
    {
        for (int i = CardsOnGameList.Count - 1; i >= 0; i--)
        {
            ResetCardServerRpc(CardsOnGameList[i]);
        }
    }

    [ServerRpc]
    public void ResetCardServerRpc(int p_cardIndex)
    {
        DealCardsClientRpc(p_cardIndex, Player.DEFAULT);
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
        UsableItemsList[p_itemID].gameObject.transform.SetPositionAndRotation(m_itemsSO.InitialPosition, Quaternion.Euler(m_itemsSO.InitialRotation));
        UsableItemsList[p_itemID].gameObject.transform.SetParent(m_itemDeckParent.transform, true);
    }

    /// <summary>
    /// About Items Use
    /// </summary>
    /// 
    private void OnItemUsed(object p_itemIndex, EventArgs p_args)
    {
        Debug.Log($"{(Player)OwnerClientId} at OnItemUsed - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        int l_itemID = (int)p_itemIndex;
        PlayersHaveItem[(int)UsableItemsList[l_itemID].playerId] = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseItemServerRpc(int p_itemIndex)
    {
        Debug.Log("UseItemServerRpc");
        switch (UsableItemsList[p_itemIndex].type)
        {
            case ItemType.NONE:
                break;
            case ItemType.SCISSORS:
                UseScissorServerRpc(UsableItemsList[p_itemIndex].playerId);
                break;
            case ItemType.STAKE:
                UseStakeServerRpc(UsableItemsList[p_itemIndex].playerId);
                break;
        }

    }

    List<int> l_cardsToRemove = new List<int>();
    List<int> l_tempCardList = new List<int>();
    [ServerRpc(RequireOwnership = false)]
    public void UseScissorServerRpc(Player p_playerId)
    {
        Card l_card;

        if (l_cardsToRemove == null) l_cardsToRemove = new();
        else l_cardsToRemove.Clear();

        for (int i = 0; i < CardsOnGameList.Count; i++)
        {
            l_card = GetCardByIndex(CardsOnGameList[i]);
            if (l_card.playerId == p_playerId && l_card.isOnGame) l_cardsToRemove.Add(CardsOnGameList[i]);
        }

        int l_quantityOfCardsRemoved = l_cardsToRemove.Count;

        l_tempCardList = UsableDeckList.Where(card => card.playerId == Player.DEFAULT).Select(card => card.id).ToList();

        for (int i = CardsOnGameList.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < l_cardsToRemove.Count; j++)
            {
                if (CardsOnGameList[i] == l_cardsToRemove[j])
                {
                    int l_cardID = CardsOnGameList[i];
                    ResetCardServerRpc(l_cardID);

                    break;
                }
            }
        }

        for (int i = 0; i < l_quantityOfCardsRemoved; i++)
        {
            int l_rand;
            do
            {
                l_rand = UnityEngine.Random.Range(0, l_tempCardList.Count);
            }
            while (GetCardByIndex(l_rand).playerId != Player.DEFAULT);

            DealCardsClientRpc(l_rand, p_playerId, i + 1 == l_quantityOfCardsRemoved);
        }

        SetHasItemClientRpc((int)p_playerId, false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseStakeServerRpc(Player p_playerId)
    {
        Debug.Log("UseStakeServerRpc");
        InformPlayerClientRpc((int)p_playerId);
        SetHasItemClientRpc((int)p_playerId, false);
    }
    Card l_card;
    [ClientRpc]
    private void InformPlayerClientRpc(int p_playerId)
    {
        Debug.Log("InformPlayerClientRpc" + PlayerController.LocalInstance.PlayerIndex + " " + p_playerId);
        if (p_playerId == PlayerController.LocalInstance.PlayerIndex)
        {
            for (int i = 0; i < CardsOnGameList.Count; i++)
            {
                l_card = GetCardByIndex(CardsOnGameList[i]);
                if (l_card.playerId != (Player)p_playerId && l_card.isOnGame)
                {
                    Debug.Log("adversary suit: " + l_card.suit);
                }
            }
        }
    }

    public void GetOtherPlayerSuits(int p_playerRequired, ref List<Suit> p_suits)
    {
        p_suits.Clear();
        for (int i = 0; i < CardsOnGameList.Count; i++)
        {
            l_card = GetCardByIndex(CardsOnGameList[i]);
            if (l_card.playerId != (Player)p_playerRequired && l_card.isOnGame)
            {
                if (!p_suits.Contains(l_card.suit)) p_suits.Add(l_card.suit);
            }
        }
    }

	public event EventHandler OnRoundWon;
    private void RetractCards(object p_wonRound, EventArgs p_args)
    {
        StartCoroutine(IRetractCards(p_wonRound));

    }

    float l_retractCards = 2f;
    IEnumerator IRetractCards(object p_wonRound)
    {
        Debug.Log("recolhe as cards");
        int l_winnerID = (int)p_wonRound;

        ReturnEyes?.Invoke(p_wonRound, EventArgs.Empty);

        yield return new WaitForSeconds(l_retractCards);
        Debug.Log("cabo de recolher as cards");
        yield return null;

        OnRoundWon?.Invoke(p_wonRound, EventArgs.Empty);
    }

    

}
