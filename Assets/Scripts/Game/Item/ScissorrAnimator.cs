using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScissorAnimator : MonoBehaviour
{
    [System.Serializable]
    public struct ScissorBoneData
    {
        public Vector3 boneZeroRotation;
        public Vector3 boneOneRotation;
        public AnimationCurve m_animCurve;
        public float m_animTime;
    }

    public ScissorBoneData openedBoneData;
    public ScissorBoneData closedBoneData;

    public Transform boneZero;
    public Transform boneOne;

    public void CloseScissors()
    {

    }

    

}
