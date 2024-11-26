using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_lobbyNameText;
    private Lobby m_lobby;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            LobbyManager.Instance.IdJoin(m_lobby.Id);
        });
    }

    public void SetLobby(Lobby p_lobby)
    {
        m_lobby = p_lobby;
        m_lobbyNameText.text = p_lobby.Name;
    }

}
