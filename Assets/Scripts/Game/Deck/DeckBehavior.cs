using System;
using UnityEngine;

public class DeckBehavior : MonoBehaviour
{
    public void GiveUp(Action<GameObject> p_onFinishAnim)
    {
        Debug.Log("[GAME] GiveUp");
        p_onFinishAnim?.Invoke(gameObject);
    }
}