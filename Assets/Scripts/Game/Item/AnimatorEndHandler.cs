using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEndHandler : MonoBehaviour
{
    public System.Action OnEndedAnim;
    public System.Action OnDeliverItemAnim;
    public void EndedAnim()
    {
        OnEndedAnim?.Invoke();
    }

    public void DeliverItem()
    {
        OnDeliverItemAnim?.Invoke();
    }
}
