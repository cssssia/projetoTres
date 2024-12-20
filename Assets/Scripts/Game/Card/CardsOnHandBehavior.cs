using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardsOnHandBehavior : MonoBehaviour
{
    [SerializeField] private bool m_invertZPlayer;
    [SerializeField] private List<CardBehavior> m_cardsBehavior;
    public List<CardBehavior> CardBehaviors => m_cardsBehavior;
    private CardBehavior m_currentHoverCard;
    private CardBehavior m_currentHoldingCard;
    [SerializeField] private Image m_throwCardTargetImage;
    [SerializeField] private CardThrowTargetTag m_throwCardThrowTargetTag;

    [Header("Idle")]
    [SerializeField] private Vector3 m_idleScale;
    [SerializeField] private float m_handWidthPerCard;
    [SerializeField] private int m_cardsQuantity = 3;

    [Header("Target")]
    [SerializeField] private List<Transform> m_targets;
    [SerializeField] private CardTransform[] m_targetsTransform;
    [SerializeField] private int m_currentTargetIndex;
    public int CurrentTargetIndex { get { return m_currentTargetIndex; } }
    PointerEventData m_pointerEventData;
    private void Start()
    {
        //SetCardsIdlePosition(true);
        m_pointerEventData = new PointerEventData(EventSystem.current);


    }

    private PlayerController m_player;
    public void OnPlayerSpawned(PlayerController p_playerController)
    {
        m_player = p_playerController;

        if (!m_player.IsOwner) return;

        m_throwCardThrowTargetTag = FindObjectOfType<CardThrowTargetTag>();
        m_throwCardTargetImage = m_throwCardThrowTargetTag.targetImage;
        m_throwCardTargetImage.gameObject.SetActive(false);
    }

    CardBehavior l_card;
    public void AddCardOnHand(int p_cardIndex, bool p_lastCard)
    {
        CardsManager.Instance.GetCardByIndex(p_cardIndex).gameObject.TryGetComponent(out l_card);

        if (m_cardsBehavior == null) m_cardsBehavior = new();

        bool l_hasItem = false;
        for (int i = 0; i < m_cardsBehavior.Count; i++) if (m_cardsBehavior[i].item != null) l_hasItem = true;

        if (!l_hasItem) m_cardsBehavior.Add(l_card);
        else m_cardsBehavior.Insert(0, l_card);

        l_card.SetCardData(CardsManager.Instance.GetCardByIndex(p_cardIndex));

        if (p_lastCard)
        {
            SetCardsIdlePosition(false);
            AnimCardsDealing();
        }
    }

    public void AddItemOnHand(int p_itemIndex)
    {
        if (CardsManager.Instance.GetItemByIndex(p_itemIndex).gameObject.TryGetComponent(out l_card))
        {
            l_card.SetCardData(CardsManager.Instance.GetItemByIndex(p_itemIndex));

            m_cardsBehavior.Add(l_card);
        }

        SetCardsIdlePosition(false);
        StartCoroutine(AnimSingleCardDeal(l_card, () => { RoundManager.Instance.OnEndedDealingItemServerRpc(m_player.PlayerIndex); }));
    }

    [NaughtyAttributes.Button]
    public void DEBUG_SetCardsPOs()
    {
        SetCardsIdlePosition(true);
    }

    public void SetCardsIdlePosition(bool p_alsoSetPosition)
    {
        if (m_cardsQuantity == 1)
        {
            if (p_alsoSetPosition) m_cardsBehavior[0].transform.localPosition = Vector3.zero;
        }
        else if (m_cardsQuantity > 1)
        {
            float l_totalWidht = m_cardsQuantity * m_handWidthPerCard;
            for (int i = 0; i < m_cardsBehavior.Count; i++)
            {
                if (m_cardsQuantity < i) return;
                float l_axisPosition = (l_totalWidht / (float)m_cardsQuantity) * i - l_totalWidht / 2f;

                Vector3 l_newPos = new Vector3(l_axisPosition, 0, 0.01f * i);
                CardTransform l_transform = new(l_newPos, new Vector3(270f, 0, 0), m_idleScale);

                m_cardsBehavior[i].SetIdleTransform(l_transform, m_invertZPlayer);
                if (p_alsoSetPosition) m_cardsBehavior[i].transform.localPosition = l_newPos;
            }
        }
    }
    [NaughtyAttributes.Button]
    public void DEBUG_CardDealing() => AnimCardsDealing();
    private void AnimCardsDealing()
    {
        for (int i = 0; i < m_cardsBehavior.Count; i++)
        {
            if (m_cardsBehavior[i].item == null) m_cardsBehavior[i].ResetTransform();
        }

        GameManager.Instance.SetPlayerAnimatingServerRpc(true);
        StartCoroutine(AnimCardsDeal());
    }


    List<int> l_tempCardsIDOnHand;
    IEnumerator AnimCardsDeal()
    {
        if (GameManager.Instance.nextGameState.Value is GameManager.GameState.HostTurn && m_player.IsClientPlayer
            || GameManager.Instance.nextGameState.Value is GameManager.GameState.ClientTurn && m_player.IsHostPlayer)
            yield return new WaitForSeconds(0.25f);

        if (l_tempCardsIDOnHand == null) l_tempCardsIDOnHand = new();
        l_tempCardsIDOnHand.Clear();
        for (int i = 0; i < m_cardsBehavior.Count; i++)
        {
            if (m_cardsBehavior[i].item == null || m_cardsBehavior[i].item.type is ItemType.NONE)
            {
                l_tempCardsIDOnHand.Add(i);
            }
            else
            {
                yield return m_cardsBehavior[i].AnimToIdlePos();
            }
        }

        for (int i = 0; i < l_tempCardsIDOnHand.Count; i++)
        {
            yield return AnimSingleCardDeal(m_cardsBehavior[l_tempCardsIDOnHand[i]]);

            if (i + 1 < l_tempCardsIDOnHand.Count) yield return new WaitForSeconds(.5f);
        }

        GameManager.Instance.SetPlayerAnimatingServerRpc(false);
        RoundManager.Instance.OnEndedDealingCardsServerRpc(m_player.PlayerIndex);
    }

    IEnumerator AnimSingleCardDeal(CardBehavior p_card, Action p_actionOnEnd = null)
    {
        yield return p_card.AnimToIdlePos(CardAnimType.DEAL);
        p_actionOnEnd?.Invoke();
    }

    [NaughtyAttributes.Button]
    public void ReorderCards(CardBehavior p_ignoreCard = null)
    {
        SetCardsIdlePosition(false);

        for (int i = 0; i < m_cardsBehavior.Count; i++)
        {
            if (m_cardsBehavior[i] == p_ignoreCard) continue;
            m_cardsBehavior[i].AnimToIdlePos();
        }
    }

    public bool CheckHoverObject(GameObject p_gameObject)
    {
        if (m_currentHoldingCard != null) return false;
        if (!m_player.CanPlay) return false;

        bool l_isCard = false;
        if (p_gameObject != null)
        {
            for (int i = 0; i < m_cardsBehavior.Count; i++)
            {
                if (m_cardsBehavior[i] != null && m_cardsBehavior[i].gameObject == p_gameObject)
                {
                    if (m_cardsBehavior[i] != m_currentHoverCard && m_cardsBehavior[i].CurrentState is not CardAnimType.PLAY)
                    {
                        m_currentHoverCard = m_cardsBehavior[i];
                        m_cardsBehavior[i].HighlightCard();
                    }

                    l_isCard = true;
                }
            }
        }

        if (!l_isCard)
        {
            if (m_currentHoverCard != null)
            {
                m_currentHoverCard.HighlightOff();
                m_currentHoverCard = null;
            }
        }
        else
        {
            for (int i = 0; i < m_cardsBehavior.Count; i++)
            {
                if (m_cardsBehavior[i] != m_currentHoverCard) m_cardsBehavior[i].HighlightOff();
            }
        }

        return l_isCard;
    }

    public void UpdateMousePos(Vector3 p_mousePos)
    {
        if (m_currentHoldingCard != null)
        {
            m_currentHoldingCard.DragCard(p_mousePos);
        }
    }

    public bool CheckClickObject(GameObject p_gameObject)
    {
        if (!m_player.CanPlay) return false;

        bool l_isCard = false;
        if (p_gameObject != null)
        {
            for (int i = 0; i < m_cardsBehavior.Count; i++)
            {
                if (m_cardsBehavior[i].gameObject == p_gameObject)
                {
                    m_currentHoldingCard = m_cardsBehavior[i];
                    m_currentHoldingCard.StartDrag(Input.mousePosition);
                    m_throwCardTargetImage.gameObject.SetActive(true);

                    l_isCard = true;
                }
            }
        }

        return l_isCard;
    }

    List<RaycastResult> m_resultList;
    public void CheckClickUp(bool p_canPlay, Action<int, bool> p_actionOnStartAnimation, Action<GameObject> p_actionOnEndAnimation,
                            Action<GameObject> p_actionOnEndItemAnim)
    {
        if (m_currentHoldingCard != null)
        {
            m_pointerEventData.position = Input.mousePosition;

            if (m_resultList == null) m_resultList = new List<RaycastResult>();
            else m_resultList.Clear();

            EventSystem.current.RaycastAll(m_pointerEventData, m_resultList);

            bool l_playCard = false;

            if (p_canPlay && !GameManager.Instance.IsAnyAnimationPlaying.Value)
            {
                for (int i = 0; i < m_resultList.Count; i++)
                {
                    if (m_resultList[i].gameObject == m_throwCardTargetImage.gameObject)
                    {
                        if (m_currentHoldingCard.item == null || m_currentHoldingCard.item.type == ItemType.NONE)
                        {
                            p_actionOnStartAnimation.Invoke(m_currentHoldingCard.card.id, false);
                            PlayCard(m_currentHoldingCard, p_actionOnEndAnimation);
                            l_playCard = true;
                            m_throwCardTargetImage.gameObject.SetActive(false);
                        }
                        else
                        {
                            p_actionOnStartAnimation.Invoke(m_currentHoldingCard.item.id, true);
                            UseItem(m_currentHoldingCard, p_actionOnEndItemAnim);
                            l_playCard = true;
                            m_throwCardTargetImage.gameObject.SetActive(false);

                        }
                        break;
                    }
                }
            }

            if (!l_playCard)
            {
                m_currentHoldingCard.EndDrag();
                m_currentHoldingCard = null;
            }
            else
            {
                ReorderCards(m_currentHoldingCard);
            }

            m_throwCardTargetImage.gameObject.SetActive(false);
        }
    }

    public void ResetCardsOnHandBehavior()
    {
        m_currentHoldingCard = null;
        for (int i = m_cardsBehavior.Count - 1; i >= 0; i--)
        {
            if (m_cardsBehavior[i].card == null) continue;

            m_cardsBehavior.RemoveAt(i);
        }
        m_currentTargetIndex = 0;
    }

    public void RemoveCardFromHand(int p_cardID)
    {
        for (int i = m_cardsBehavior.Count - 1; i >= 0; i--)
        {
            if (m_cardsBehavior[i].card == null) continue;

            if (m_cardsBehavior[i].card == CardsManager.Instance.GetCardByIndex(p_cardID))
            {
                // m_cardsBehavior[i].OnDestroyAction -= RemoveNullCardsFromList;
                m_cardsBehavior.RemoveAt(i);
                break;
            }
        }
    }

    public bool RemoveCard(CardBehavior p_card)
    {
        return m_cardsBehavior.Remove(p_card);
    }

    public void AddTarget(Transform p_target, int p_targetIndex)
    {
        if (m_targets == null || m_targets.Count < 3)
        {
            m_targets = new List<Transform>();
            for (int i = 0; i < 3; i++) m_targets.Add(null);
        }

        m_targets[p_targetIndex] = p_target;
    }

    private void PlayCard(CardBehavior cardBehavior, Action<GameObject> p_action)
    {
        GameManager.Instance.SetPlayerAnimatingServerRpc(true);
        cardBehavior.PlayCard(GetNextCardTarget(), p_action);
        m_currentHoldingCard = null;
        m_cardsBehavior.Remove(cardBehavior);
        m_currentTargetIndex++;
    }

    private void UseItem(CardBehavior p_cardBehavior, Action<GameObject> p_action)
    {
        GameManager.Instance.SetPlayerAnimatingServerRpc(true);
        p_cardBehavior.PlayCard(CardsManager.Instance.ItemTarget, p_action);
        m_currentHoldingCard = null;
        m_cardsBehavior.Remove(p_cardBehavior);
    }

    private CardTransform GetNextCardTarget()
    {
        if (m_targetsTransform.Length < m_targets.Count)
        {
            m_targetsTransform = new CardTransform[m_targets.Count];

            for (int i = 0; i < m_targetsTransform.Length; i++)
            {
                Vector3 l_tempRot = m_targets[i].eulerAngles;
                //l_tempRot.x -= 180f;
                m_targetsTransform[i] = new(m_targets[i].position, (l_tempRot), Vector3.one * CardBehavior.CARD_TABLE_SCALE);
            }
        }
        return m_targetsTransform[m_currentTargetIndex];
    }

    // void OnDestroy()
    // {
    //     Debug.Log("OnDestroy");
    //     Debug.Log((Player)m_player.PlayerIndex);
    //     Debug.Log(m_cardsBehavior.Count);
    //     for (int i = 0; i < m_cardsBehavior.Count; i++)
    //     {
    //         m_cardsBehavior[i].OnDestroyAction -= RemoveNullCardsFromList;
    //     }
    // }

    [NaughtyAttributes.Button]
    public void DEBUG_AnimCardCut()
    {
        AnimCardCutPosition(null);
    }

    public void AnimCardCutPosition(Action<GameObject> p_actionOnEndAnim)
    {
        for (int i = 0; i < m_cardsBehavior.Count; i++)
            if (m_cardsBehavior[i].item.type is ItemType.NONE) m_cardsBehavior[i].AnimToCutPosition(i, p_actionOnEndAnim);
    }

    public void AnimCardDestroy()
    {
        for (int i = 0; i < m_cardsBehavior.Count; i++)
            if (m_cardsBehavior[i].item.type is ItemType.NONE) m_cardsBehavior[i].AnimCardDestroy();

    }

    [NaughtyAttributes.Button]
    public void DEBUG_ResetDestroy()
    {
        for (int i = 0; i < m_cardsBehavior.Count; i++)
            if (m_cardsBehavior[i].item.type is ItemType.NONE) m_cardsBehavior[i].SetShaderHide(0f);

    }
}
