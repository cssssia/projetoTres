using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CFG_DeckAnim_", menuName = "ScriptableObjects/Deck Anim")]
public class DeckAnimConfig : ScriptableObject
{
    [SerializeField] private float m_animTime;
    public float AnimTime { get => m_animTime; }
    public AnimationCurve MoveAnimCurve;

}
