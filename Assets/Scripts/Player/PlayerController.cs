using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// C:\Users\Usuario\AppData\LocalLow\DefaultCompany\projetoTres > Player.log

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set;}

    [SerializeField] private List<Vector3> m_spawnPositionList;

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

    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId)
        {
            Debug.Log("owner disconnected");
            // destroy network stuff
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;
    }
}
