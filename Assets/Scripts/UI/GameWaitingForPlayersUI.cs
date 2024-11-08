using System;
using UnityEngine;

public class GameWaitingForPlayersUI : MonoBehaviour
{

    void Start()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        Hide();
    }

    private void GameManager_OnLocalPlayerReadyChanged(object p_sender, EventArgs e)
    {
        if (GameManager.Instance.IsLocalPlayerReady())
            Show();
    }

    private void GameManager_OnGameStateChanged(object p_sender, EventArgs e)
    {
        if (!GameManager.Instance.IsWaitingToStart())
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
