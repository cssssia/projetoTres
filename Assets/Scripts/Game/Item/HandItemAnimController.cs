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
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.ShowIf("followObject")] public AnimationCurve followCurve;
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.DisableIf("followObject")]
        public Vector3 targetPosition;
        //[NaughtyAttributes.AllowNesting, NaughtyAttributes.DisableIf("followObject")]
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.HideIf("followObject")] public float yPump;
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.HideIf("followObject")] public AnimationCurve yPumpCurve;

        [Space] public bool quaternionLerp;
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

    [Space]
    public HatchController hatchController;

    [Space]
    public StakeAnimator stakeAnimator;
    public Material stakeMaterial;
    public BetHandAnimator betHandAnimator;
    [Space]

    public System.Action OnCutCards;
    public System.Action OnEndedScissorAnim;
    public System.Action OnImpaleCards;
    public System.Action OnEndedStakeAnim;

    [NaughtyAttributes.Button]
    public void UseScissors() => HandItem((int)PlayerType, ItemType.SCISSORS, null);

    [NaughtyAttributes.Button]
    public void UseStake() => HandItem((int)PlayerType, ItemType.STAKE, null);

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
        handAnimator.SetTrigger("Idle");
        hatchController = FindObjectOfType<HatchController>();
    }

    public void HandItem(int p_playerID, ItemType p_item, System.Action p_onEnd)
    {
        switch (p_item)
        {
            case ItemType.SCISSORS:
                handAnimator.SetTrigger("UseItemScissors");
                break;
            case ItemType.STAKE:
                if (p_playerID == 0) handAnimator.SetTrigger("HostUseItemStake");
                else handAnimator.SetTrigger("ClientUseItemStake");
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
    Vector3 l_initPosition, l_initialRotation, l_tempPosition;
    Quaternion l_initQuaternion, l_finalQuaternion;
    IEnumerator ExecuteAnimQueue(ObjectOnHandAnim p_object, int p_playerID, System.Action p_onEnd)
    {
        p_object.InitialPosition = p_object.ObjectTranform.position;
        p_object.InitialRotation = p_object.ObjectTranform.eulerAngles;

        if (p_object.Type is ItemType.SCISSORS) p_object.endHandler.OnEndedAnim += OnEndScissorCutAnim;
        if (p_object.Type is ItemType.STAKE)
        {
            stakeMaterial.SetFloat("_AlphaClipThreshold",0f);
            stakeMaterial.SetFloat("_DissolutionOn", 0f);
        }

        yield return hatchController.OpenHatch();

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
            else
            {
                if (l_currentAnimData[i].followCurve.length == 0) l_initPosition = l_currentAnimData[i].objectToFollow.position;
                else l_initPosition = p_object.ObjectTranform.position;
            }

            l_initQuaternion = p_object.ObjectTranform.localRotation;
            l_initialRotation = p_object.ObjectTranform.localRotation.eulerAngles;
            l_finalQuaternion = Quaternion.Euler(l_currentAnimData[i].targetRotation);

            while (l_time <= l_maxTime && l_maxTime > 0)
            {
                float t = l_time / l_maxTime;
                if (!l_followObject)
                {
                    l_tempPosition = Vector3.Lerp(l_initPosition, l_currentAnimData[i].targetPosition,
                                                                            l_currentAnimData[i].curve.Evaluate(t));
                    l_tempPosition.y += l_currentAnimData[i].yPump * l_currentAnimData[i].yPumpCurve.Evaluate(t);

                    p_object.ObjectTranform.localPosition = l_tempPosition;
                    //p_object.ObjectTranform.localEulerAngles = Vector3.Lerp(l_initialRotation,
                    //                                                    l_currentAnimData[i].targetRotation,
                    //                                                    l_currentAnimData[i].curve.Evaluate(l_time / l_maxTime));
                }
                else
                {
                    if (l_currentAnimData[i].followCurve.length == 0)
                        p_object.ObjectTranform.position = l_currentAnimData[i].objectToFollow.position;
                    else p_object.ObjectTranform.position = Vector3.Lerp(l_initPosition, l_currentAnimData[i].objectToFollow.position,
                                                                                            l_currentAnimData[i].followCurve.Evaluate(t));

                }

                if (l_currentAnimData[i].quaternionLerp)
                    p_object.ObjectTranform.localRotation = Quaternion.Lerp(l_initQuaternion, l_finalQuaternion,
                                                                            l_currentAnimData[i].curve.Evaluate(l_time / l_maxTime));
                else p_object.ObjectTranform.localRotation = Quaternion.Euler(Vector3.Lerp(l_initialRotation,
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
            {
                if (l_currentAnimData[i].eventToInvokeOnEnd == "stakeImpact") OnImpactStake();
                else p_object.itemAnimator.SetTrigger(l_currentAnimData[i].eventToInvokeOnEnd);
            }
        }

        yield return hatchController.CloseHatch();

        ResetObject(p_object);

        if (p_object.Type is ItemType.SCISSORS) OnEndedScissorAnim?.Invoke();
        if (p_object.Type is ItemType.STAKE) OnEndedStakeAnim?.Invoke();

        p_onEnd?.Invoke();
    }

    void OnEndScissorCutAnim()
    {
        OnCutCards?.Invoke();
    }

    List<Suit> m_suitsToHighlight;
    public void SetSuitsToHighlight(List<Suit> p_suits)
    {
        m_suitsToHighlight = p_suits;
    }

    void OnImpactStake()
    {
        stakeAnimator.HighlightSymbols(m_suitsToHighlight);
    }

    void OnEndStakeImpaleAnim()
    {
        OnImpaleCards?.Invoke();
    }

    void ResetObject(ObjectOnHandAnim p_object)
    {
        p_object.ObjectTranform.position = p_object.InitialPosition;
        p_object.ObjectTranform.rotation = Quaternion.Euler(p_object.InitialRotation);
    }
}
