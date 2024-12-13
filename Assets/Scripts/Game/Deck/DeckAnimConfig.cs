using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CFG_DeckAnim_", menuName = "ScriptableObjects/Deck Anim")]
public class DeckAnimConfig : ScriptableObject
{
    [SerializeField] private float m_animTime;
    public float AnimTime { get => m_animTime; }
    public AnimationCurve MoveAnimCurve;
    public bool UseLocalPosition = true;
    [Header("Rotation")]
    public AnimationCurve RotationAnimCurve;
    public bool freezeXRotation, freezeYRotation, freezeZRotation;
    //public bool lookAtPlayer;
    //public AnimationCurve LookAtPlayerAnimCurve;

    [SerializeField] private float m_deckYPump;
    public float DeckYPump { get => m_deckYPump; }
    public AnimationCurve YPumpCurve;
}
