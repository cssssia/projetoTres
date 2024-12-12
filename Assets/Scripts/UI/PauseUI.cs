using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button m_resumeButton;
    [SerializeField] private Button m_rulesButton;
    [SerializeField] private Button m_mainMenuButton;
    [SerializeField] private Button m_optionsButton;
    [SerializeField] private GameObject m_optionsUI;
    [SerializeField] private GameObject m_pausedUI;
    [SerializeField] private GameObject m_rulesUI;

    private void Awake() {
        m_resumeButton.onClick.AddListener(() => {
            GameManager.Instance.TogglePauseGame();
        });
		m_rulesButton.onClick.AddListener(() => {
            m_pausedUI.SetActive(false);
            m_rulesUI.SetActive(true);
        });
        m_mainMenuButton.onClick.AddListener(() => {
			NetworkManager.Singleton.Shutdown(); // do this whenever the player quits the party, game over etc
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneLoader.Scene.SCN_Menu.ToString());
        });
        m_optionsButton.onClick.AddListener(() => {
            m_pausedUI.SetActive(false);
            m_optionsUI.SetActive(true);
        });
    }

	void Start()
	{
		GameManager.Instance.OnLocalGamePaused += GameManager_OnLocalGamePaused;
		GameManager.Instance.OnLocalGameUnpaused += GameManager_OnLocalGameUnpaused;

		Hide();
	}

	private void GameManager_OnLocalGamePaused(object p_sender, EventArgs e)
	{
		Show();
	}

	private void GameManager_OnLocalGameUnpaused(object p_sender, EventArgs e)
	{
        m_rulesUI.SetActive(false);
        m_optionsUI.SetActive(false);
        m_pausedUI.SetActive(true);
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