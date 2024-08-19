using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// TODO options menu

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button m_resumeButton;
    [SerializeField] private Button m_mainMenuButton;
    [SerializeField] private Button m_optionsButton;

    private void Awake() {
        m_resumeButton.onClick.AddListener(() => {
            GameManager.Instance.TogglePauseGame();
        });
        m_mainMenuButton.onClick.AddListener(() => {
			NetworkManager.Singleton.Shutdown(); // do this whenever the player quits the party, game over etc
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneLoader.Scene.SCN_Menu.ToString());
        });
        m_optionsButton.onClick.AddListener(() => {
            Hide();
            //OptionsUI.Instance.Show(Show);
        });
    }

	void Start()
	{
		GameManager.Instance.OnLocalGamePaused += GameManager_OnLocalGamePaused;
		GameManager.Instance.OnLocalGameUnpaused += GameManager_OnLocalGameUnpaused;

		Hide();
	}

	private void GameManager_OnLocalGamePaused(object sender, EventArgs e)
	{
		Show();
	}

	private void GameManager_OnLocalGameUnpaused(object sender, EventArgs e)
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