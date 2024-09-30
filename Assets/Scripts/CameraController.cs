using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class CameraController : Singleton<CameraController>
{
    [SerializeField] private Camera m_camera;
    private int m_cameraIndex;
    [SerializeField] private CameraState m_currentState = CameraState.HAND;
    [SerializeField] private CameraAnimConfig[] m_animConfigs;

    private void Start()
    {
        GameInput.Instance.OnButtonVerticalDown += GameInput_Down;
        GameInput.Instance.OnButtonVerticalUp += GameInput_Up;
    }

    public void GameInput_Up(object sender, EventArgs p_eventArgs)
    {
        switch (m_currentState)
        {
            case CameraState.HAND:
                if (m_cameraAnimCoroutine != null) StopCoroutine(m_cameraAnimCoroutine);
                m_cameraAnimCoroutine = StartCoroutine(AnimCamera(CameraState.TABLE));
                break;
        }
    }

    public void GameInput_Down(object sender, EventArgs p_eventArgs)
    {
        switch (m_currentState)
        {
            case CameraState.TABLE:
                if (m_cameraAnimCoroutine != null) StopCoroutine(m_cameraAnimCoroutine);
                m_cameraAnimCoroutine = StartCoroutine(AnimCamera(CameraState.HAND));
                break;
        }
    }

    Coroutine m_cameraAnimCoroutine;
    Vector3 l_tempRotation, l_initialRotation, l_finalRotation, l_initialPosition, l_tempPosition, l_finalPosition;
    CameraAnimData m_cameraData;
    IEnumerator AnimCamera(CameraState p_state)
    {
        if (m_currentState == p_state) yield break;
        m_currentState = p_state;

        l_initialPosition = m_camera.transform.localPosition;
        m_cameraData = m_animConfigs[m_cameraIndex].GetAnimData(p_state);
        l_finalPosition = m_cameraData.LocalPosition;
        l_initialRotation = m_camera.transform.localRotation.eulerAngles;
        l_finalRotation = m_cameraData.LocalRotation;

        float l_animTime = m_animConfigs[m_cameraIndex].AnimTime;
        float l_time = 0f;

        while (l_time <= l_animTime)
        {
            float l_evaluate = l_time / l_animTime;
            l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, l_finalPosition, m_cameraData.PositionCurve.Evaluate(l_evaluate));

            l_tempRotation.x = Mathf.LerpAngle(l_initialRotation.x, l_finalRotation.x, m_cameraData.RotationCurve.Evaluate(l_evaluate));
            l_tempRotation.y = Mathf.LerpAngle(l_initialRotation.y, l_finalRotation.y, m_cameraData.RotationCurve.Evaluate(l_evaluate));
            l_tempRotation.z = Mathf.LerpAngle(l_initialRotation.z, l_finalRotation.z, m_cameraData.RotationCurve.Evaluate(l_evaluate));

            m_camera.transform.localPosition = l_tempPosition;
            m_camera.transform.localRotation = Quaternion.Euler(l_tempRotation);

            l_time += Time.deltaTime;
            yield return null;
        }

        l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, l_finalPosition, 1f);

        l_tempRotation.x = Mathf.LerpAngle(l_initialRotation.x, l_finalRotation.x, 1f);
        l_tempRotation.y = Mathf.LerpAngle(l_initialRotation.y, l_finalRotation.y, 1f);
        l_tempRotation.z = Mathf.LerpAngle(l_initialRotation.z, l_finalRotation.z, 1f);

        m_camera.transform.SetLocalPositionAndRotation(l_tempPosition, Quaternion.Euler(l_tempRotation));

        m_cameraAnimCoroutine = null;
    }


    public void SetCamera(int p_index)
    {
        m_cameraIndex = p_index;
        m_camera.transform.SetLocalPositionAndRotation(
            m_animConfigs[p_index].GetAnimData(CameraState.HAND).LocalPosition,
               Quaternion.Euler(m_animConfigs[p_index].GetAnimData(CameraState.HAND).LocalRotation));
    }

#if UNITY_EDITOR
    public void SaveCurrentAnimData()
    {
        m_animConfigs[0].SetAnimData(m_currentState, m_camera.transform.localPosition, m_camera.transform.localEulerAngles);
        EditorUtility.SetDirty(m_animConfigs[0]);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Save"))
        {
            (target as CameraController).SaveCurrentAnimData();
        }
    }
}
#endif