using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandItemAnimController : MonoBehaviour
{
    [System.Serializable]
    public class ObjectOnHandAnim
    {
        public ItemType Type;
        [Space]
        public Transform ObjectTranform;
        public Animator itemAnimator;
        public AnimatorEndHandler endHandler;
        [Space]
        public Vector3 InitialPosition;
        public Vector3 InitialRotation;
        public List<AnimData> animDataHost;
        public List<AnimData> animDataClient;

        public int index;
        [NaughtyAttributes.Button]
        public void GetPositionHost()
        {
            animDataHost[index].targetPosition = ObjectTranform.localPosition;
        }
        [NaughtyAttributes.Button]
        public void GetPositionClient()
        {
            animDataClient[index].targetPosition = ObjectTranform.localPosition;
        }
    }

    [System.Serializable]
    public class AnimData
    {
        public float wait;
        public bool followObject;
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.EnableIf("followObject")]
        public Transform objectToFollow;
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.DisableIf("followObject")]
        public Vector3 targetPosition;
        //[NaughtyAttributes.AllowNesting, NaughtyAttributes.DisableIf("followObject")]
        public Vector3 targetRotation;
        public float time;
        public AnimationCurve curve;
        public string eventToInvokeOnStart;
        public string eventToInvokeOnEnd;
    }
    public Player PlayerType;

    public Animator handAnimator;
    public AnimatorEndHandler handAnimatorEndHandler;
    public ObjectOnHandAnim[] objectsOnHand;

    public System.Action OnCutCards;
    public System.Action OnEndedScissorAnim;
    public System.Action OnImpaleCards;
    public System.Action OnEndedStakeAnim;

    [NaughtyAttributes.Button]
    public void UseScissors() => HandItem((int)PlayerType, ItemType.SCISSORS, null);

    [NaughtyAttributes.Button]
    public void GetPositionClient()
    {
        objectsOnHand[0].GetPositionClient();
    }
    [NaughtyAttributes.Button]
    public void GetPositionHost()
    {
        objectsOnHand[0].GetPositionHost();
    }

    private void Start()
    {
        handAnimator.SetTrigger("idle");
    }

    public void HandItem(int p_playerID, ItemType p_item, System.Action p_onEnd)
    {
        switch (p_item)
        {
            case ItemType.SCISSORS:
                handAnimator.SetTrigger("UseItemScissors");
                break;
            case ItemType.STAKE:
                Debug.Log("UseItemStake");
                break;

        }
        //handAnimator.SetTrigger("UseItem");

        for (int i = 0; i < objectsOnHand.Length; i++)
        {
            if (objectsOnHand[i].Type == p_item)
            {
                StartCoroutine(ExecuteAnimQueue(objectsOnHand[i], p_playerID, p_onEnd));
                break;
            }
        }
    }


    List<AnimData> l_currentAnimData;
    Vector3 l_initPosition, l_initialRotation;
    IEnumerator ExecuteAnimQueue(ObjectOnHandAnim p_object, int p_playerID, System.Action p_onEnd)
    {
        p_object.InitialPosition = p_object.ObjectTranform.position;
        p_object.InitialRotation = p_object.ObjectTranform.eulerAngles;

        if (p_object.Type is ItemType.SCISSORS) p_object.endHandler.OnEndedAnim += OnEndScissorCutAnim;
        if (p_object.Type is ItemType.STAKE) p_object.endHandler.OnEndedAnim += OnEndStakeImpaleAnim;

        l_currentAnimData = p_playerID == 0 ? p_object.animDataHost : p_object.animDataClient;
        for (int i = 0; i < l_currentAnimData.Count; i++)
        {
            if (l_currentAnimData[i].wait > 0) yield return new WaitForSeconds(l_currentAnimData[i].wait);

            if (l_currentAnimData[i].eventToInvokeOnStart != string.Empty)
                p_object.itemAnimator.SetTrigger(l_currentAnimData[i].eventToInvokeOnStart);

            float l_time = 0f;
            float l_maxTime = l_currentAnimData[i].time;

            bool l_followObject = l_currentAnimData[i].followObject;

            if (!l_followObject) l_initPosition = p_object.ObjectTranform.localPosition;
            else l_initPosition = l_currentAnimData[i].objectToFollow.position;

            l_initialRotation = p_object.ObjectTranform.localRotation.eulerAngles;

            while (l_time <= l_maxTime && l_maxTime > 0)
            {
                if (!l_followObject)
                {

                    p_object.ObjectTranform.localPosition = Vector3.Lerp(l_initPosition,
                                                                            l_currentAnimData[i].targetPosition,
                                                                            l_currentAnimData[i].curve.Evaluate(l_time / l_maxTime));

                    //p_object.ObjectTranform.localEulerAngles = Vector3.Lerp(l_initialRotation,
                    //                                                    l_currentAnimData[i].targetRotation,
                    //                                                    l_currentAnimData[i].curve.Evaluate(l_time / l_maxTime));
                }
                else
                {
                    p_object.ObjectTranform.position = l_currentAnimData[i].objectToFollow.position;

                }


                    p_object.ObjectTranform.localRotation =Quaternion.Euler(Vector3.Lerp(l_initialRotation,
                                                                        l_currentAnimData[i].targetRotation,
                                                                        l_currentAnimData[i].curve.Evaluate(l_time / l_maxTime)));

                yield return null;
                l_time += Time.deltaTime;
            }

            if (!l_followObject)
                p_object.ObjectTranform.localPosition = Vector3.Lerp(l_initPosition, l_currentAnimData[i].targetPosition, 1f);
            else p_object.ObjectTranform.position = l_currentAnimData[i].objectToFollow.position;
            p_object.ObjectTranform.localRotation = Quaternion.Euler(Vector3.Lerp(l_initialRotation,
                                                                       l_currentAnimData[i].targetRotation,
                                                                       l_currentAnimData[i].curve.Evaluate(1f)));

            if (l_currentAnimData[i].eventToInvokeOnEnd != string.Empty)
                p_object.itemAnimator.SetTrigger(l_currentAnimData[i].eventToInvokeOnEnd);
        }

        ResetObject(p_object);

        if (p_object.Type is ItemType.SCISSORS) OnEndedScissorAnim?.Invoke();
        if (p_object.Type is ItemType.STAKE) OnEndedStakeAnim?.Invoke();

        p_onEnd?.Invoke();
    }

    void OnEndScissorCutAnim()
    {
        Debug.Log("cut");
        OnCutCards?.Invoke();
    }

    void OnEndStakeImpaleAnim()
    {
        Debug.Log("impale");
        OnImpaleCards?.Invoke();
    }

    void ResetObject(ObjectOnHandAnim p_object)
    {
        p_object.ObjectTranform.position = p_object.InitialPosition;
        p_object.ObjectTranform.rotation = Quaternion.Euler(p_object.InitialRotation);
    }
}
