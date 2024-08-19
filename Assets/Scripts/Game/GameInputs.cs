using System;
using UnityEngine;

public class GameInput : MonoBehaviour {

    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnPauseAction;

    private void Awake() {
        Instance = this;
    }

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