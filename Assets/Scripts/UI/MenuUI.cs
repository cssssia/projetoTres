using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject m_menuGameObject;
    [SerializeField] private Button m_playButton;
    [SerializeField] private Button m_optionsButton;
    [SerializeField] private Button m_quitButton;

    [Header("Options Menu")]
    [SerializeField] private GameObject m_optionsGameObject;
    [SerializeField] private Slider m_masterSlider;
    [SerializeField] private Slider m_musicSlider;
    [SerializeField] private Slider m_sfxSlider;
    [SerializeField] private TMP_Dropdown m_lenguageDropdown;
    [SerializeField] private Button m_backButton;

    void Awake()
    {
        Time.timeScale = 1f;

        m_playButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            SceneLoader.Load(SceneLoader.Scene.SCN_Lobby);
        });
        m_optionsButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            m_menuGameObject.SetActive(false);
            m_optionsGameObject.SetActive(true);
        });
        m_quitButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            Application.Quit();
        });
        m_backButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            m_menuGameObject.SetActive(true);
            m_optionsGameObject.SetActive(false);
        });
        m_masterSlider.onValueChanged.AddListener((p_value) => {
            AudioManager.Instance.masterVolume = p_value;
        });
        m_musicSlider.onValueChanged.AddListener((p_value) => {
            AudioManager.Instance.musicVolume = p_value;
        });
        m_sfxSlider.onValueChanged.AddListener((p_value) => {
            AudioManager.Instance.sfxVolume = p_value;
        });
        m_lenguageDropdown.onValueChanged.AddListener((p_value) => {
            Debug.Log(p_value);
        });

        m_optionsGameObject.SetActive(false);
    }


}
