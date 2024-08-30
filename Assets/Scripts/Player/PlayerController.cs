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
        Debug.Log("click");
        RaycastHit l_raycastHit;

        Ray l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(l_ray, out l_raycastHit, 100f))
        {
            if (l_raycastHit.transform != null)
            {
                //Our custom method. 
                CurrentClickedGameObject(l_raycastHit.transform.gameObject);
            }
        }
    }

    private void CurrentClickedGameObject(GameObject gameObject)
    {
        Debug.Log("uar");
        if(gameObject.tag=="something")
        {
            Debug.Log(gameObject.name);
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;
    }
}
