using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button m_createGameButton;
    [SerializeField] private Button m_joinGameButton;

    void Awake()
    {
        m_createGameButton.onClick.AddListener(() => {
            GameMultiplayerManager.Instance.StartHost();
            SceneLoader.LoadNetwork(SceneLoader.Scene.SCN_WaitLobby);
        });

        m_joinGameButton.onClick.AddListener(() => {
            GameMultiplayerManager.Instance.StartClient();
            // dont need to start scene here as it automatically matches server scene
        });

    }
}
