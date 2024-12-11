using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }
    public const int matchEndValue = 15;

    public NetworkVariable<bool> MatchHasEnded;
    public NetworkVariable<Player> WonMatch;

    [SerializeField] private List<GameObject> m_pointsHost;
    [SerializeField] private List<GameObject> m_pointsClient;

    void Awake()
    {
        Instance = this;

        MatchHasEnded.Value = false;
        WonMatch.Value = Player.DEFAULT;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"{(Player)OwnerClientId} at MatchManager - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        RoundManager.Instance.PointsHost.OnValueChanged += PointsHost_OnValueChanged;
        RoundManager.Instance.PointsClient.OnValueChanged += PointsClient_OnValueChanged;
    }

    private void PointsHost_OnValueChanged(int p_previousValue, int p_newValue)
    {
        Debug.Log("b");
        for (int i = p_previousValue; i < p_newValue; i++)
        {
            m_pointsHost[i].SetActive(true);
        }

        if (p_newValue >= matchEndValue)
            EndMatchServerRpc(Player.HOST);
    }

    private void PointsClient_OnValueChanged(int p_previousValue, int p_newValue)
    {

        Debug.Log("a");
        for (int i = p_previousValue; i < p_newValue; i++)
        {
            m_pointsClient[i].SetActive(true);
        }

        if (p_newValue >= matchEndValue)
            EndMatchServerRpc(Player.CLIENT);
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndMatchServerRpc(Player p_playerWonMatch)
    {
        Debug.Log($"[GAME] {p_playerWonMatch} Won Match");

        WonMatch.Value = p_playerWonMatch;
        MatchHasEnded.Value = true;
    }

}