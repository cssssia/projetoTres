using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button m_closeButton;
    [SerializeField] private Button m_createPublicButton;
    [SerializeField] private Button m_createPrivateButton;
    [SerializeField] private TMP_InputField m_lobbyNameInputField;

    void Start()
    {
        Hide();
    }

    void Awake()
    {

        m_closeButton.onClick.AddListener(() => {
            Hide();
        });

        m_createPublicButton.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(m_lobbyNameInputField.text, false);
        });

        m_createPrivateButton.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(m_lobbyNameInputField.text, true);
        });

    }

    public void Show()
	{
		gameObject.SetActive(true);
	}

	private void Hide()
    {
        gameObject.SetActive(false);
    }

}
