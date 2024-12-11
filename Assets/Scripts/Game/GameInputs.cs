using System;
using UnityEngine;

public class GameInput : Singleton<GameInput>
{

    public event EventHandler OnInteractAction;
    public event EventHandler OnStopInteractAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnMoveMouse;
    public event EventHandler OnButtonVerticalUp;
    public event EventHandler OnButtonVerticalDown;

    Vector3 m_lastMousePosition;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Interact();
        if (Input.GetMouseButtonUp(0))
            StopInteract();

        if (MatchManager.Instance != null && !MatchManager.Instance.MatchHasEnded.Value && Input.GetKeyDown(KeyCode.Escape))
            Pause();

        if (Input.mousePosition != m_lastMousePosition)
            MoveMouse();

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            VerticalAxis(true);

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            VerticalAxis(false);
    }

    private void Pause()
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact()
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void StopInteract()
    {
        OnStopInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void MoveMouse()
    {
        m_lastMousePosition = Input.mousePosition;
        OnMoveMouse?.Invoke(m_lastMousePosition, EventArgs.Empty);
    }

    private void VerticalAxis(bool p_up)
    {
        if (p_up) OnButtonVerticalUp?.Invoke(this, EventArgs.Empty);
        else OnButtonVerticalDown?.Invoke(this, EventArgs.Empty);
    }
}