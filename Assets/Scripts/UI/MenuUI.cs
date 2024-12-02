using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Button m_playButton;
    [SerializeField] private Button m_optionsButton;
    [SerializeField] private Button m_quitButton;
    [SerializeField] private GameObject m_optionsUI;

    void Awake()
    {
        Time.timeScale = 1f;

        m_playButton.onClick.AddListener(() => {
            // AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            SceneLoader.Load(SceneLoader.Scene.SCN_Lobby);
        });
        m_optionsButton.onClick.AddListener(() => {
            // AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            Hide();
            m_optionsUI.SetActive(true);
        });
        m_quitButton.onClick.AddListener(() => {
            // AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            Application.Quit();
        });
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
