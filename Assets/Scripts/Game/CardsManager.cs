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
    public List<string> deck;
    private int m_deckIndex;

    private float m_dealDeckTimer = 4f;
    private float m_dealDeckTimerMax = 4f;

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

            if (deck.Count == 0)
            {
                //PlayCards();
                SpawnNewPlayCardsServerRpc();
            }
        }

    }

    [ServerRpc]
    void SpawnNewPlayCardsServerRpc() //can only instantiate prefabs on server
    {
        //StartCoroutine(Deal());
        deck = GenerateDeck();
        foreach (string card in deck)
        {
            GameObject newCard = Instantiate(m_cardPrefab, m_cardPrefab.transform.position, m_cardPrefab.transform.rotation);
            NetworkObject cardNetworkObject = newCard.GetComponent<NetworkObject>();
            cardNetworkObject.Spawn(true);
            m_deckCards.Add(cardNetworkObject);
            RenameCardClientRpc(cardNetworkObject);
            //newCard.name = card;
        }

    }

    int i;
    [ClientRpc]
    void RenameCardClientRpc(NetworkObject cardNetworkObject)
    {
        deck = GenerateDeck();
        Debug.Log("RenameCardClientRpc");
        foreach (string card in deck)
        {
            Debug.Log("rn" + card);
            cardNetworkObject.name = card;
            Debug.Log("n" + cardNetworkObject.name);
        }
    }

    public void PlayCards()
    {
        deck = GenerateDeck();
        Shuffle(deck);
        //StartCoroutine(Deal());
    }

    public static List<string> GenerateDeck()
    {
        List<string> newDeck = new List<string>();
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                newDeck.Add(s + v);
            }
        }
        return newDeck;
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

    IEnumerator Deal()
    {
        foreach (string card in deck)
        {
            yield return new WaitForSeconds(0.01f);
            GameObject newCard = Instantiate(m_cardPrefab, transform.position, Quaternion.identity);
            NetworkObject cardNetworkObject = newCard.GetComponent<NetworkObject>();
            cardNetworkObject.Spawn(true);
            newCard.name = card;
        }
    }
}
