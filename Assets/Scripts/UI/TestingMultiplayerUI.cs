using UnityEngine;
using UnityEngine.UI;

public class TestingMultiplayerUI : MonoBehaviour
{
	public static TestingMultiplayerUI Instance { get; private set;}

    [SerializeField] private Button m_hostButton;
    [SerializeField] private Button m_clientButton;

    void Awake()
    {
        Instance = this;

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
