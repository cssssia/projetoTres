using UnityEngine;
using UnityEngine.UI;

public class TestingMultiplayerUI : Singleton<TestingMultiplayerUI>
{
    [SerializeField] private Button m_hostButton;
    [SerializeField] private Button m_clientButton;

    void Awake()
    {

        m_hostButton.onClick.AddListener(() => {
            GameMultiplayerManager.Instance.StartHost();
            Hide();
        });

        m_clientButton.onClick.AddListener(() => {
            GameMultiplayerManager.Instance.StartClient();
            Hide();
        });

    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
