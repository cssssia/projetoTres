using System;
using UnityEngine;

//not being used
public class GameCountdownToStartUI : MonoBehaviour
{

    void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object p_sender, EventArgs e)
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
