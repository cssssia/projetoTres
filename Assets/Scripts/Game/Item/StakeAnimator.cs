using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StakeAnimator : MonoBehaviour
{
   public struct SuitSymbol
    {
        public Suit suit;
        public Material material;
    }


    public SuitSymbol[] symbols;
    public AnimationCurve animCurve;
    public float animTime;

    public void HighlightSymbols(List<Suit> p_suits)
    {
        for (int i = 0; i < symbols.Length; i++)
        {
            if (p_suits.Contains(symbols[i].suit)) StartCoroutine(HiglightAnim(symbols[i].material));
        }
    }

    IEnumerator HiglightAnim(Material p_material)
    {
        yield return null;
    }
}
