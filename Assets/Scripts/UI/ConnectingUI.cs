using System;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{

    void Start()
    {
        MultiplayerManager.Instance.OnTryingToJoinGame += MultiplayerManager_OnTryingToJoinGame;
        MultiplayerManager.Instance.OnFailToJoinGame += MultiplayerManager_OnFailToJoinGame;
        Hide();
    }

    void OnDestroy()
    {
        MultiplayerManager.Instance.OnTryingToJoinGame -= MultiplayerManager_OnTryingToJoinGame;
        MultiplayerManager.Instance.OnFailToJoinGame -= MultiplayerManager_OnFailToJoinGame;
    }

    private void MultiplayerManager_OnTryingToJoinGame(object p_sender, EventArgs e)
    {
        Show();
    }

    private void MultiplayerManager_OnFailToJoinGame(object p_sender, EventArgs e)
    {
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
