using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Button m_playButton;
    [SerializeField] private Button m_quitButton;

    void Awake()
    {
        m_playButton.onClick.AddListener(() => {
            SceneLoader.Load(SceneLoader.Scene.SCN_Lobby);
        });
        m_quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }
}
