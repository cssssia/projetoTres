using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAnimatorEndHandler : MonoBehaviour
{
    public System.Action OnEndedAnim;
    public void EndedAnim()
    {
        OnEndedAnim?.Invoke();
    }
}
