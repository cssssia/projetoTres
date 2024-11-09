using System;
using UnityEngine;

[Serializable]
public class BetTransform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public BetTransform() { }
    public BetTransform(Vector3 p_position, Vector3 p_rotation, Vector3 p_scale)
    {
        Position = p_position;
        Rotation = p_rotation;
        Scale = p_scale;
    }
    public BetTransform(CardTransform p_cardTransform)
    {
        Position = p_cardTransform.Position;
        Rotation = p_cardTransform.Rotation;
        Scale = p_cardTransform.Scale;
    }
}

public enum BetAnimType { PLAY, IDLE, HIGHLIGHT, DRAG }
public class BetBehavior : MonoBehaviour
{
    public int playerId;
    private BetAnimType m_currentState = BetAnimType.IDLE;
    private Vector3 m_startPosition;
    private Vector3 m_startRotation;

    private void Start()
    {
        m_startPosition = transform.position;
        m_startRotation = transform.rotation.eulerAngles;
    }

    public void AnimateToPlace(bool p_isIncrease, Action<GameObject, bool> p_action = null)
    {
        p_action?.Invoke(gameObject, p_isIncrease);
    }

    bool m_dragging;
    private Vector3 l_startMousePos;
    public void StartDrag(Vector3 p_mousePos)
    {
        //transform.localScale = Vector3.one * 0.075f;
        m_dragging = true;
        m_currentState = BetAnimType.DRAG;
        //l_startMousePos = p_mousePos - Camera.main.WorldToScreenPoint(transform.position);
    }

    private Vector3 l_tempDragPos;
    public void DragBet(Vector3 p_mousePosition, RaycastHit p_raycastHit)
    {
        if (!m_dragging) return;

        l_tempDragPos = p_raycastHit.point;
        //l_tempDragPos = Camera.main.ScreenToWorldPoint(p_mousePosition - l_startMousePos);
        //l_tempDragPos.y = transform.position.y;

        transform.position = l_tempDragPos;
    }

    public void EndDrag()
    {
        m_dragging = false;
    }
}