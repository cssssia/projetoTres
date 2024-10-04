using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

[CreateAssetMenu(fileName = "CFG_CardAnim_", menuName = "ScriptableObjects/Camera Anim")]

public class CameraAnimConfig : ScriptableObject
{
   [SerializeField] private CameraAnimData[] m_animData;
    public float AnimTime = 0.5f;

    private void InitCameraData()
    {
        m_animData = new CameraAnimData[3];

        for (int i = 0; i < m_animData.Length; i++) { m_animData[i] = new((CameraState)i); }
    }

    public void SetAnimData(CameraState p_state, Vector3 p_position, Vector3 p_rotation)
    {
        if(m_animData.Length == 0) InitCameraData();

       int l_index = (int)p_state;

        m_animData[l_index].LocalPosition = p_position;
        m_animData[l_index].LocalRotation = p_rotation;
    }

    public CameraAnimData GetAnimData(CameraState p_state)
    {
        return m_animData[(int)p_state];
    }
}

public enum CameraState { HAND, TABLE, ITEMS}

[System.Serializable]
public class CameraAnimData
{
    public CameraState State;

    [Header("Position")]
    public Vector3 LocalPosition;
    public AnimationCurve PositionCurve;

    [Header("Rotation")]
    public Vector3 LocalRotation;
    public AnimationCurve RotationCurve;

    public CameraAnimData(CameraState p_state)
    {
        State = p_state;
    }
}
