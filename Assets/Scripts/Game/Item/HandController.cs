using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [System.Serializable]
    public class ObjectOnHandAnim
    {
        public ItemType Type;
        [Space]
        public Transform ObjectTranform;
        //public GameObject parentBone;

        public Animator itemAnimator;
        //public ObjectAnimationBehaviour m_animation;
    }

    public Animator handAnimator;
    public ObjectOnHandAnim[] objectsOnHand;

    public void HandItem(ItemType p_item)
    {
        handAnimator.SetTrigger("UseItem");
        for (int i = 0; i < objectsOnHand.Length; i++)
        {
            if(objectsOnHand[i].Type == p_item)
            {
                objectsOnHand[i].itemAnimator.SetTrigger("UseItem");
                break;
            }
        }
    }
}
