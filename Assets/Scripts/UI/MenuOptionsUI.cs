using System.Collections.Generic;
using Assets.SimpleLocalization.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptionsUI : MonoBehaviour
{
    [SerializeField] private GameObject m_menuGameObject;
    [SerializeField] private Slider m_masterSlider;
    [SerializeField] private Slider m_musicSlider;
    [SerializeField] private Slider m_sfxSlider;
    [SerializeField] private TMP_Dropdown m_lenguageDropdown;
    [SerializeField] private Button m_backButton;
    [SerializeField] private List<string> m_availableLenguages = new List<string>();

    void Awake()
    {
        m_backButton.onClick.AddListener(() => {
            // AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPressed, transform.position);
            m_menuGameObject.SetActive(true);
            Hide();
        });
        m_masterSlider.onValueChanged.AddListener((p_value) => {
            // AudioManager.Instance.masterVolume = p_value;
        });
        m_musicSlider.onValueChanged.AddListener((p_value) => {
            // AudioManager.Instance.musicVolume = p_value;
        });
        m_sfxSlider.onValueChanged.AddListener((p_value) => {
            // AudioManager.Instance.sfxVolume = p_value;
        });
        m_lenguageDropdown.onValueChanged.AddListener((p_value) => {
            Localization.Instance.SetLocalization(m_availableLenguages[p_value]);
        });
    }

    void Start()
    {
        m_lenguageDropdown.value = IdentifyLenguage();
        Hide();
    }

	public void Show()
	{
		gameObject.SetActive(true);
	}

	private void Hide()
    {
        gameObject.SetActive(false);
    }

    int IdentifyLenguage()
    {
        string l_atualLanguage = LocalizationManager.Language;
        for (int i = 0; i < m_availableLenguages.Count; i++)
        {
            if (m_availableLenguages[i].Equals(l_atualLanguage))
            {
                return i;
            }
        }
        return 0;
    }

}
