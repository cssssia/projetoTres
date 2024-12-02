using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }
    public const int matchEndValue = 15;

    public NetworkVariable<bool> MatchHasEnded;
    public NetworkVariable<Player> WonMatch;
    [SerializeField] private TextMeshPro m_pointsHost;
    [SerializeField] private TextMeshPro m_pointsClient;

    void Awake()
    {
        Instance = this;

        MatchHasEnded.Value = false;
        WonMatch.Value = Player.DEFAULT;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"{(Player)OwnerClientId} at MatchManager - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        if (IsOwner)
        {
            Debug.Log("IsOwner");
            RoundManager.Instance.PointsHost.OnValueChanged += PointsHost_OnValueChanged;
            RoundManager.Instance.PointsClient.OnValueChanged += PointsClient_OnValueChanged;
        }
    }

    private void PointsHost_OnValueChanged(int p_previousValue, int p_newValue)
    {
        m_pointsHost.text = p_newValue.ToString();

        if (p_newValue >= matchEndValue)
            EndMatchServerRpc(Player.HOST);
    }

    private void PointsClient_OnValueChanged(int p_previousValue, int p_newValue)
    {
        m_pointsClient.text = p_newValue.ToString();

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