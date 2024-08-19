using System;
using UnityEngine;

public class TestingWaitingForPlayersUI : MonoBehaviour
{

    void Start()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnLocalPlayerReadyChanged(object p_sender, EventArgs e)
    {
        if (GameManager.Instance.IsLocalPlayerReady())
            Show();
    }

    private void GameManager_OnStateChanged(object p_sender, EventArgs e)
    {
        if (GameManager.Instance.IsCountdownToStartActive())
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
