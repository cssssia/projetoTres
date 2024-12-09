using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StakeAnimator : MonoBehaviour
{
    [System.Serializable]
    public class SuitSymbol
    {
        public Suit suit;
        public Material material;
        public SuitAnimData[] animData;
    }
    [System.Serializable]
    public struct SuitAnimData
    {
        public bool setColor;
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.ShowIf("setColor")] public Color initialColor;
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.ShowIf("setColor")] public Color finalColor;

        public bool setEmission;
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.ShowIf("setEmission"), ColorUsage(showAlpha: true, hdr: true)]
        public Color initialEmission;
        [NaughtyAttributes.AllowNesting, NaughtyAttributes.ShowIf("setEmission"), ColorUsage(showAlpha: true, hdr: true)]
        public Color finalEmission;
        [Space]
        public AnimationCurve animCurve;
        public float animTime;
    }

    public SuitSymbol[] symbols;
    [NaughtyAttributes.MinMaxSlider(-3.2f, 1f)] public Vector2 m_minMaxCustomHide;
    public AnimationCurve impactCurve;
    public float impactTime;
    public Material stakeMaterial;

    [Header("Dissolution Anim")]
    [SerializeField] private float m_dissolutionAnimTime;
    [SerializeField] private AnimationCurve m_dissolutionCurve;

    public void HighlightSymbols(List<Suit> p_suits)
    {
        StartCoroutine(AnimImpact(p_suits));
    }

    IEnumerator AnimImpact(List<Suit> p_suits)
    {
        float l_impactTime = 0f;
        while (l_impactTime < impactTime)
        {
            stakeMaterial.SetFloat("_CrackThickness", Mathf.LerpUnclamped(m_minMaxCustomHide.y, m_minMaxCustomHide.x, l_impactTime / impactTime));

            yield return null;
            l_impactTime += Time.deltaTime;
        }

        int l_animCount = 0;
        for (int i = 0; i < symbols.Length; i++)
        {
            if (p_suits == null || p_suits.Contains(symbols[i].suit))
            {
                if ((p_suits == null && l_animCount == 3) || (p_suits != null && l_animCount == p_suits.Count - 1)) yield return StartCoroutine(HiglightAnim(symbols[i]));
                else StartCoroutine(HiglightAnim(symbols[i]));

                l_animCount++;
            }
        }

        stakeMaterial.SetFloat("_AlphaClipThreshold", 0f);
        stakeMaterial.SetFloat("_DissolutionOn", 1f);

        float l_dissolutionTime = 0f;
        while (l_dissolutionTime < m_dissolutionAnimTime)
        {
            stakeMaterial.SetFloat("_AlphaClipThreshold",
                                    Mathf.Clamp(m_dissolutionCurve.Evaluate(l_dissolutionTime / m_dissolutionAnimTime), 0f, 1f));

            yield return null;
            l_dissolutionTime += Time.deltaTime;
        }

        stakeMaterial.SetFloat("_AlphaClipThreshold", 1f);

        yield return null;


    }

    IEnumerator HiglightAnim(SuitSymbol p_symbol)
    {
        p_symbol.material.EnableKeyword("_EMISSION");

        Color l_initEmission;
        Color l_initColor;
        for (int i = 0; i < p_symbol.animData.Length; i++)
        {
            float l_time = 0f;

            l_initEmission = p_symbol.animData[i].initialEmission;
            l_initColor = p_symbol.animData[i].initialColor;

            p_symbol.material.color = l_initEmission;
            while (l_time < p_symbol.animData[i].animTime)
            {
                float l_t = p_symbol.animData[i].animCurve.Evaluate(l_time / p_symbol.animData[i].animTime);

                if (p_symbol.animData[i].setColor)
                    p_symbol.material.color = Color.LerpUnclamped(l_initColor, p_symbol.animData[i].finalColor, l_t);

                p_symbol.material.SetColor("_EmissionColor",
                        Color.LerpUnclamped(l_initEmission, p_symbol.animData[i].finalEmission, l_t));
                l_time += Time.deltaTime;
                yield return null;
            }
        }
    }

    public List<Suit> DebugSuits;
    [NaughtyAttributes.Button]
    public void Debug_Highlight()
    {
        HighlightSymbols(DebugSuits);
    }
}
