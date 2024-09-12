using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAnimConfig : ScriptableObject
{
    [SerializeField] private float m_animTime;
    public float AnimTime { get => m_animTime; }
    public AnimationCurve MoveAnimCurve;
    public AnimationCurve RotationAnimCurve;
    [SerializeField] private float m_cardYPump;
    public float CardYPump { get => m_cardYPump; }
    public AnimationCurve YPumpCurve;
}
