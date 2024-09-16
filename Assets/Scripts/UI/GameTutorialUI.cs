using System;
using UnityEngine;

public class GameTutorialUI : MonoBehaviour
{

	void Start()
	{
		GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;

		Show();
	}

	private void GameManager_OnLocalPlayerReadyChanged(object p_sender, EventArgs e)
	{
		if (GameManager.Instance.IsLocalPlayerReady()) //done this way by the tutorial guy to give the ability to unready the player, if we want
			Hide();
	}

	private void Show()
	{
		gameObject.SetActive(true);
	}

	private void Hide()
    {
        gameObject.SetActive(false);
    }

}