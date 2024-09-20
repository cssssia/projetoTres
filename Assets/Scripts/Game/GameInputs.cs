using System;
using UnityEngine;

public class GameInput : Singleton<GameInput> {

    public event EventHandler OnInteractAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnMoveMouse;

    Vector3 m_lastMousePosition;
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
			Interact();

		if (Input.GetKeyDown(KeyCode.Escape))
			Pause();

        if (Input.mousePosition != m_lastMousePosition) 
            MoveMouse();
	}

    private void Pause() {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact() {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void MoveMouse()
    {
        m_lastMousePosition = Input.mousePosition;
        OnMoveMouse?.Invoke(m_lastMousePosition, EventArgs.Empty);
    }

}