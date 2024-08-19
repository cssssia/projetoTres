using System;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{

    void Start()
    {
        GameMultiplayerManager.Instance.OnTryingToJoinGame += GameMultiplayerManager_OnTryingToJoinGame;
        GameMultiplayerManager.Instance.OnFailToJoinGame += GameMultiplayerManager_OnFailToJoinGame;
        Hide();
    }

    void OnDestroy()
    {
        GameMultiplayerManager.Instance.OnTryingToJoinGame -= GameMultiplayerManager_OnTryingToJoinGame;
        GameMultiplayerManager.Instance.OnFailToJoinGame -= GameMultiplayerManager_OnFailToJoinGame;
    }

    private void GameMultiplayerManager_OnTryingToJoinGame(object p_sender, EventArgs e)
    {
        Show();
    }

    private void GameMultiplayerManager_OnFailToJoinGame(object p_sender, EventArgs e)
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
