using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEndHandler : MonoBehaviour
{
    public void EndedAnim(int p_itemID)
    {
        Debug.Log("Ended anim " + (ItemType)p_itemID);
    }
}
