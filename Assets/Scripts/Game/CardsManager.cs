using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CardsManager : NetworkBehaviour
{
    public static CardsManager Instance;
    public Sprite[] cardFaces;
    [SerializeField] private GameObject m_cardPrefab;
    [SerializeField] private List<NetworkObject> m_deckCards;

    [Header("Host Card")]
    [SerializeField] private Vector3 m_hostCardPosition;
    [SerializeField] private Quaternion m_hostCardRotation;

    [Header("Client Card")]
    [SerializeField] private Vector3 m_clientCardPosition;
    [SerializeField] private Quaternion m_clientCardRotation;
    private static string[] suits = new string[] { "C", "D", "H", "S" };
    private static string[] values = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    private float m_dealDeckTimer = 4f;
    private float m_dealDeckTimerMax = 4f;
    private bool m_cardsWereSpawned = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    void Start()
    {
        // Debug.Log(deck);
    }

    void Update()
    {
        if (!IsServer)
            return;

        m_dealDeckTimer -= Time.deltaTime;
        if (m_dealDeckTimer <= 0f) //timer to have time to connect host and client (only for tests)
        {
            m_dealDeckTimer = m_dealDeckTimerMax;

            if (!m_cardsWereSpawned)
                SpawnNewPlayCardsServerRpc();

        }

    }

    [ServerRpc]
    void SpawnNewPlayCardsServerRpc() //can only instantiate prefabs on server
    {
        //PlayCards();
        List<string> l_deck = GenerateDeck();
        Shuffle(l_deck);

        foreach (string card in l_deck)
        {
            GameObject newCard = Instantiate(m_cardPrefab, m_cardPrefab.transform.position, m_cardPrefab.transform.rotation);
            NetworkObject cardNetworkObject = newCard.GetComponent<NetworkObject>();
            cardNetworkObject.Spawn(true);
            RenameCardClientRpc(cardNetworkObject, card);
        }

        m_cardsWereSpawned = true;

    }

    // int i;
    [ClientRpc]
    void RenameCardClientRpc(NetworkObjectReference cardNetworkObjectReference, string card)
    {
        cardNetworkObjectReference.TryGet(out NetworkObject cardNetworkObject);
        cardNetworkObject.name = card;
    }

    // public void PlayCards()
    // {
    //     deck = GenerateDeck();
    //     Shuffle(deck);
    //     StartCoroutine(Deal());
    // }

    public static List<string> GenerateDeck()
    {
        List<string> l_newDeck = new List<string>();
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                l_newDeck.Add(s + v);
            }
        }
        return l_newDeck;
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

    // IEnumerator Deal()
    // {
    //     foreach (string card in deck)
    //     {
    //         yield return new WaitForSeconds(0.01f);
    //     }
    // }
}
