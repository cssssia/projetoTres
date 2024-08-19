using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//cannot set a network object to a parent that was dinamically spawned -> limitation

public class CardsManager : NetworkBehaviour
{
    public static CardsManager Instance;
    public Sprite[] cardFaces;
    [SerializeField] private GameObject m_cardPrefab;
    [SerializeField] private GameObject m_deckParent;
    [SerializeField] private List<NetworkObject> m_deckCards;

    private static string[] suits = new string[] { "C", "D", "H", "S" };
    private static string[] values = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    private float m_dealDeckTimer = 4f;
    private float m_dealDeckTimerMax = 4f;
    private bool m_cardsWereSpawned = false;

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

        m_dealDeckTimer -= Time.deltaTime;
        if (m_dealDeckTimer <= 0f) //timer to have time to connect host and client (only for tests)
        {
            m_dealDeckTimer = m_dealDeckTimerMax;

            if (!m_cardsWereSpawned)
                SpawnNewPlayCardsServerRpc();

        }

    }

    [ServerRpc] //[ServerRpc(RequireOwnership = false)] clients can call the function, but it runs on the server
    void SpawnNewPlayCardsServerRpc() //can only instantiate prefabs on server AND only destroy on server
    {
        //PlayCards();
        List<string> l_deck = GenerateDeck();
        Shuffle(l_deck);

        foreach (string card in l_deck)
        {
            GameObject newCard = Instantiate(m_cardPrefab, transform.position, Quaternion.identity);
            NetworkObject cardNetworkObject = newCard.GetComponent<NetworkObject>();
            cardNetworkObject.Spawn(true);
            RenameCardServerRpc(cardNetworkObject, card);
        }

        m_cardsWereSpawned = true;

    }

    [ServerRpc]
    void RenameCardServerRpc(NetworkObjectReference cardNetworkObjectReference, string card) //for a pattern, maybe ? (the tutorial guy does it)
    {
        RenameCardClientRpc(cardNetworkObjectReference, card);
    }

    // int i;
    [ClientRpc]
    void RenameCardClientRpc(NetworkObjectReference cardNetworkObjectReference, string card)
    {
        cardNetworkObjectReference.TryGet(out NetworkObject cardNetworkObject);
        cardNetworkObject.name = card;
        cardNetworkObject.TrySetParent(m_deckParent, false); //false to ignore WorldPositionStays and to work as we are used to (also do it on the client to sync position)
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
