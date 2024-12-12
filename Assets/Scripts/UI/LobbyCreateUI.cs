using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    private const string PLACEHOLDER_LOBBY_NAME = "insert lobby name";
    private const string LOBBY_NAME = "lobby ";
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
            LobbyManager.Instance.CreateLobby(m_lobbyNameInputField.text == "" ? LOBBY_NAME + Random.Range(100, 1000) : m_lobbyNameInputField.text, false);
        });

        m_createPrivateButton.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(m_lobbyNameInputField.text == "" ? LOBBY_NAME + Random.Range(100, 1000) : m_lobbyNameInputField.text, true);
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
