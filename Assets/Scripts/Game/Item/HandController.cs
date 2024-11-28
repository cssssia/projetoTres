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
        public Animator itemAnimator;
        public ItemAnimatorEndHandler endHandler;
        [Space]
        public Vector3 InitialPosition;
        public Vector3 InitialRotation;
        public List<AnimData> animData;
    }

    [System.Serializable]
    public class AnimData
    {
        public float wait;
        public Vector3 targetPosition;
        public Vector3 targetRotation;
        public float time;
        public AnimationCurve curve;
        public string eventToInvokeOnStart;
        public string eventToInvokeOnEnd;
    }

    public Animator handAnimator;
    public ObjectOnHandAnim[] objectsOnHand;

    public System.Action OnCutCards;
    public System.Action OnEndedScissorAnim;

    [NaughtyAttributes.Button]
    public void UseScissors() => HandItem(ItemType.SCISSORS, null);

    public void HandItem(ItemType p_item, System.Action p_onEnd)
    {
        handAnimator.SetTrigger("UseItem");
        for (int i = 0; i < objectsOnHand.Length; i++)
        {
            if (objectsOnHand[i].Type == p_item)
            {
                StartCoroutine(ExecuteAnimQueue(objectsOnHand[i], p_onEnd));
                break;
            }
        }
    }

    Vector3 l_initPosition, l_initialRotation;
    IEnumerator ExecuteAnimQueue(ObjectOnHandAnim p_object, System.Action p_onEnd)
    {
        p_object.InitialPosition = p_object.ObjectTranform.position;
        p_object.InitialRotation = p_object.ObjectTranform.eulerAngles;

        if (p_object.Type is ItemType.SCISSORS) p_object.endHandler.OnEndedAnim += OnEndScissorCutAnim;

        for (int i = 0; i < p_object.animData.Count; i++)
        {
            if (p_object.animData[i].wait > 0) yield return new WaitForSeconds(p_object.animData[i].wait);

            if (p_object.animData[i].eventToInvokeOnStart != string.Empty)
                p_object.itemAnimator.SetTrigger(p_object.animData[i].eventToInvokeOnStart);

            float l_time = 0f;
            float l_maxTime = p_object.animData[i].time;
            l_initPosition = p_object.ObjectTranform.localPosition;
            l_initialRotation = p_object.ObjectTranform.localRotation.eulerAngles;

            while (l_time <= l_maxTime)
            {
                p_object.ObjectTranform.localPosition = Vector3.Lerp(l_initPosition,
                                                                    p_object.animData[i].targetPosition,
                                                                    p_object.animData[i].curve.Evaluate(l_time / l_maxTime));
                p_object.ObjectTranform.localEulerAngles = Vector3.Lerp(l_initialRotation,
                                                                    p_object.animData[i].targetRotation,
                                                                    p_object.animData[i].curve.Evaluate(l_time / l_maxTime));
                yield return null;
                l_time += Time.deltaTime;
            }
            p_object.ObjectTranform.localPosition = Vector3.Lerp(l_initPosition, p_object.animData[i].targetPosition, 1f);

            if (p_object.animData[i].eventToInvokeOnEnd != string.Empty)
                p_object.itemAnimator.SetTrigger(p_object.animData[i].eventToInvokeOnEnd);
        }

        ResetObject(p_object);

        if (p_object.Type is ItemType.SCISSORS) OnEndedScissorAnim?.Invoke();

        p_onEnd?.Invoke();
    }

    void OnEndScissorCutAnim()
    {
        Debug.Log("cut");
        OnCutCards?.Invoke();
    }

    void ResetObject(ObjectOnHandAnim p_object)
    {
        p_object.ObjectTranform.position = p_object.InitialPosition;
        p_object.ObjectTranform.rotation = Quaternion.Euler(p_object.InitialRotation);
    }
}
