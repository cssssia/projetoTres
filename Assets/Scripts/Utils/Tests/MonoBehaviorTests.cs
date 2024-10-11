using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class MonoBehaviorTests : MonoBehaviour
{

    [Button]
    private void DealCardsAgain()
    {
        PlayerController.LocalInstance.RemoveAllCardsFromHandServerRpc();
    }
}
