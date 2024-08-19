using System;
using UnityEngine;

public class CountdownToStartUI : MonoBehaviour
{

    void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
		if (GameManager.Instance.IsCountdownToStartActive())
			Show();
        if (GameManager.Instance.IsGamePlaying())
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
