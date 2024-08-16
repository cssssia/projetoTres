using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// C:\Users\Usuario\AppData\LocalLow\DefaultCompany\projetoTres > Player.log

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set;}

    void Start()
    {
        if (!IsOwner)
            return;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalInstance = this;
    }

    void Update()
    {
        if (!IsOwner)
            return;
    }
}
