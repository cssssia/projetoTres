using System;
using UnityEngine;

//not being used
public class GameCountdownToStartUI : MonoBehaviour
{

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        Hide();
    }

    private void GameManager_OnGameStateChanged(object p_sender, EventArgs e)
    {
		// if (GameManager.Instance.IsCountdownToStartActive())
		// 	Show();
        // if (GameManager.Instance.IsGamePlaying())
        //     Hide();
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
