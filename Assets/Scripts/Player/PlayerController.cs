using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// C:\Users\Usuario\AppData\LocalLow\DefaultCompany\projetoTres > Player.log

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set;}

    [SerializeField] private List<Vector3> m_spawnPositionList;
    [SerializeField] private List<CardsScriptableObject.Card> m_myHand;

    void Start()
    {
        if (!IsOwner)
            return;
    }

    public override void OnNetworkSpawn() //research more the difference of this and awake
    {

        if (IsOwner)
            LocalInstance = this;

        transform.position = m_spawnPositionList[GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)]; //do this on camera later

        if (IsOwner)
            CameraController.Instance.SetCamera(GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        if (IsOwner)
    		GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

    }

    private void NetworkManager_OnClientDisconnectCallback(ulong p_clientId)
    {
        if (p_clientId == OwnerClientId)
        {
            Debug.Log("owner disconnected");
            // destroy network stuff
        }
    }

    private void GameInput_OnInteractAction(object p_sender, System.EventArgs e)
    {
        RaycastHit l_raycastHit;

        Ray l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(l_ray, out l_raycastHit, 10f))
        {
            if (l_raycastHit.transform != null)
            {
                //Our custom method.
                CurrentClickedGameObject(l_raycastHit.transform.gameObject);
            }
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;
    }

    private void CurrentClickedGameObject(GameObject gameObject)
    {
        if(gameObject.CompareTag("Card"))
        {
            Debug.Log(gameObject.name);
            for (int i = 0; i < m_myHand.Count; i++)
                if (m_myHand[i].name == gameObject.name)
                    TurnManager.Instance.PlayCard(m_myHand[i].value);
        }
    }

    public void AddToMyHand(CardsScriptableObject.Card p_card)
    {
        m_myHand.Add(p_card);
    }

}
