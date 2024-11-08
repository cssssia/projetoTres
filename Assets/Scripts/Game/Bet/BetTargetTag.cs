using UnityEngine;
using UnityEngine.UI;

public enum Type { ACCEPT, INCREASE }
public class BetTargetTag : MonoBehaviour
{
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

}