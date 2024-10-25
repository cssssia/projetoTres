using UnityEngine;
using Unity.Netcode;

public class NetworkBehaviorTests : NetworkBehaviour
{
    public static NetworkBehaviorTests Instance;
    public bool testMode = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public override void OnNetworkSpawn()
    {
        if (testMode)
        {
            Debug.Log("[TEST] Spawn");
            TestClientRpc();

            if (!IsServer)
            {
                Debug.Log("[TEST] Not Server");
                TestRequireOwnershipServerRpc();
                return;
            }

            Debug.Log("[TEST] Server");
            TestServerRpc();
        }
    }


    [ServerRpc]
    public void TestServerRpc()
    {
        Debug.Log("[TEST] TestServerRpc " + GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        if (IsOwner)
        {
            Debug.Log("[TEST] TestServerRpc & IsOwner " + GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void TestRequireOwnershipServerRpc()
    {
        Debug.Log("[TEST] TestRequireOwnershipServerRpc " + GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        if (IsOwner)
        {
            Debug.Log("[TEST] TestRequireOwnershipServerRpc & IsOwner " + GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));
        }

    }

    [ClientRpc]
    private void TestClientRpc()
    {
        Debug.Log("[TEST] TestClientRpc " + GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        if (IsOwner)
        {
            Debug.Log("[TEST] TestClientRpc & IsOwner " + GameMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));
        }

    }

}
