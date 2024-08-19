using System;
using UnityEngine;

public class GameInput : Singleton<GameInput> {

    public event EventHandler OnInteractAction;
    public event EventHandler OnPauseAction;

	void Update()
	{

		if (Input.GetMouseButtonDown(0))
			Interact();

		if (Input.GetKeyDown(KeyCode.Escape))
			Pause();

	}

    private void Pause() {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact() {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

}