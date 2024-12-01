using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Button m_playButton;
    [SerializeField] private Button m_optionsButton;
    [SerializeField] private Button m_quitButton;

    void Awake()
    {
        m_playButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            SceneLoader.Load(SceneLoader.Scene.SCN_Lobby);
        });
        m_optionsButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
        });
        m_quitButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            Application.Quit();
        });

        Time.timeScale = 1f;
    }
}
