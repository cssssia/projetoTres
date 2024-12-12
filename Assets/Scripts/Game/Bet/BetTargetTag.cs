using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Type { ACCEPT, INCREASE, BET }
public class BetTargetTag : MonoBehaviour
{
    public int playerId;
    public TextMeshPro text;
    public RectTransform targetRect;
    public Type type;
    public bool IsAccept
    {
        get { return type == Type.ACCEPT; }
    }
    public bool IsIncrease
    {
        get { return type == Type.INCREASE; }
    }

    public bool IsBet
    {
        get { return type == Type.BET ; }
    }
}