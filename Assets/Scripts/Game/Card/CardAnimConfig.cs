using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CFG_CardAnim_", menuName = "ScriptableObjects/Card Anim")]
public class CardAnimConfig : ScriptableObject
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

    [SerializeField] private float m_cardYPump;
    public float CardYPump { get => m_cardYPump; }
    public AnimationCurve YPumpCurve;
}
