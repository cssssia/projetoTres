using System.Collections;
using UnityEngine;

public class PointBehavior : MonoBehaviour
{
    private Material m_myMaterial;
    [SerializeField] private Material m_material;

    [Header("Dissolution Anim")]
    [SerializeField] private float m_dissolutionAnimTime;
    [SerializeField] private AnimationCurve m_dissolutionCurve;

    void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(m_material);
        m_myMaterial = gameObject.GetComponent<MeshRenderer>().material;
    }

    [NaughtyAttributes.Button]
    public void AnimPointShow()
    {
        StartCoroutine(IAnimPoint());
    }

    IEnumerator IAnimPoint()
    {
        float l_dissolutionTime = 0f;
        while (l_dissolutionTime < m_dissolutionAnimTime)
        {
            m_myMaterial.SetFloat("_Custom_hide",
                                    Mathf.Lerp(2.6f, 0f, m_dissolutionCurve.Evaluate(l_dissolutionTime / m_dissolutionAnimTime)));

            yield return null;
            l_dissolutionTime += Time.deltaTime;
        }
        m_myMaterial.SetFloat("_Custom_hide", 0f);
    }


}