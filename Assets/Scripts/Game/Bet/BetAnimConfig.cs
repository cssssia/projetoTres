using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CFG_BetAnim_", menuName = "ScriptableObjects/Bet Anim")]
public class BetAnimConfig : ScriptableObject
{
    [SerializeField] private float m_animTime;
    public float AnimTime { get => m_animTime; }
    public AnimationCurve MoveAnimCurve;
    public bool UseLocalPosition = true;
    public AnimationCurve RotationAnimCurve;
    [SerializeField] private float m_betZPump;
    public float betZPump { get => m_betZPump; }
    public AnimationCurve ZPumpCurve;
}
