using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProccessController : MonoBehaviour
{
    public VolumeProfile postProcessProfile;
    private ColorAdjustments m_colorAdjustments;

    [Header("Exposure")]
    [SerializeField] private float m_currentExposureFactor;
    [SerializeField, NaughtyAttributes.MinMaxSlider(0f, 7f)] private Vector2 m_minMaxExposure;
    [SerializeField] private AnimationCurve m_exposureCurve;
    [Header("Anim")]
    [SerializeField] private float m_animTime;
    [SerializeField] private AnimationCurve m_animCurve;

    private void Start()
    {
        postProcessProfile.TryGet(out m_colorAdjustments);
    }

    public void SetExposure(float p_exposture)
    {
        if (m_currentExposureFactor == p_exposture) return;
        m_currentExposureFactor = p_exposture;
        Debug.Log("set exposure " + p_exposture);
        float l_nextValue = Mathf.Lerp(m_minMaxExposure.x, m_minMaxExposure.y, m_exposureCurve.Evaluate(p_exposture));

        StartCoroutine(AnimFloatParameter(m_colorAdjustments.postExposure, l_nextValue));
    }

    IEnumerator AnimFloatParameter(FloatParameter p_floatParam, float p_nextValue)
    {
        float l_initValue = p_floatParam.value;
        float l_time = 0f;

        Debug.Log("start animing exposure " + p_floatParam.value);
        while (l_time < m_animTime)
        {
            p_floatParam.Interp(l_initValue, p_nextValue, m_animCurve.Evaluate(l_time / m_animTime));
            yield return null;
            l_time += Time.deltaTime;
        }

        p_floatParam.value = p_nextValue;
        Debug.Log("end animing exposure " + p_floatParam.value);
    }
}
