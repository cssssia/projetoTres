using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

// TODO handle empty lobby name

public class LobbyManager : MonoBehaviour
{
    //aa
    public static LobbyManager Instance { get; private set; }
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnCodeJoinFailed;
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;

    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby m_joinedLobby;
    private float m_keepAliveTimer;
    private float m_listLobbiesTimer;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeUnityAuthentication();
    }

    void Update()
    {
        LobbyKeepAlive();
        HandlePeriodicListLobbies();
    }

    private void LobbyKeepAlive()
    {
        if (IsLobbyHost())
        {
            m_keepAliveTimer -= Time.deltaTime;
            if (m_keepAliveTimer <= 0f)
            {
                float l_keepAliveTimerMax = 15f;
                m_keepAliveTimer = l_keepAliveTimerMax;
                LobbyService.Instance.SendHeartbeatPingAsync(m_joinedLobby.Id);
            }
        }
    }

    private void HandlePeriodicListLobbies()
    {
        if (m_joinedLobby == null &&
            //UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn &&
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneLoader.Scene.SCN_Lobby.ToString())
        {
            m_listLobbiesTimer -= Time.deltaTime;
            if (m_listLobbiesTimer <= 0f)
            {
                float l_listLobbiesTimerMax = 3f;
                m_listLobbiesTimer = l_listLobbiesTimerMax;
                ListLobbies();
            }
        }
    }

    private bool IsLobbyHost()
    {
        return m_joinedLobby != null && m_joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void ListLobbies()
    {
        try {

            QueryLobbiesOptions l_queryLobbiesOptions = new QueryLobbiesOptions {
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };
            QueryResponse l_queryResponse = await LobbyService.Instance.QueryLobbiesAsync(l_queryLobbiesOptions);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = l_queryResponse.Results
            });

        } catch (LobbyServiceException e) {
            Debug.Log("[ERROR] ListLobbies: " + e);
        }
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions l_initializationOptions = new InitializationOptions();
            l_initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString()); // test for multiple builds in same pc
            await UnityServices.InitializeAsync(l_initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private async Task<Allocation> AllocateRelay()
    {
        try {
            Allocation l_allocation = await RelayService.Instance.CreateAllocationAsync(MultiplayerManager.MAX_PLAYER_AMOUNT - 1);
            return l_allocation;
        } catch (RelayServiceException e) {
            Debug.Log("[ERROR] AllocateRelay: " + e);
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation p_allocation)
    {
        try {
            string l_relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(p_allocation.AllocationId);
            return l_relayJoinCode;
        } catch (RelayServiceException e) {
            Debug.Log("[ERROR] GetRelayCodeJoin: " + e);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string p_joinCode)
    {
        try {
            JoinAllocation l_joinAllocation = await RelayService.Instance.JoinAllocationAsync(p_joinCode);
            return l_joinAllocation;
        } catch (RelayServiceException e) {
            Debug.Log("[ERROR] JoinRelay: " + e);
            return default;
        }
    }

    public async void CreateLobby(string p_lobbyName, bool p_isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try {

            m_joinedLobby = await LobbyService.Instance.CreateLobbyAsync(p_lobbyName, MultiplayerManager.MAX_PLAYER_AMOUNT, new CreateLobbyOptions {
                IsPrivate = p_isPrivate,
            });

            Allocation l_allocation = await AllocateRelay();

            string l_relayJoinCode = await GetRelayJoinCode(l_allocation);

            await LobbyService.Instance.UpdateLobbyAsync(m_joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, l_relayJoinCode) }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(l_allocation, "dtls"));

            MultiplayerManager.Instance.StartHost();
            SceneLoader.LoadNetwork(SceneLoader.Scene.SCN_WaitLobby);

        } catch (LobbyServiceException e) {
            Debug.Log("[ERROR] CreateLobby: " + e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void QuickJoin()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try {

            m_joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string l_relayJoinCode = m_joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation l_joinAllocation = await JoinRelay(l_relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(l_joinAllocation, "dtls"));

            MultiplayerManager.Instance.StartClient();

        } catch (LobbyServiceException e) {
            Debug.Log("[ERROR] QuickJoin: " + e);
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void CodeJoin(string p_lobbyCode)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try {

            m_joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(p_lobbyCode);

            string l_relayJoinCode = m_joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation l_joinAllocation = await JoinRelay(l_relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(l_joinAllocation, "dtls"));

            MultiplayerManager.Instance.StartClient();

        } catch (LobbyServiceException e) {
            Debug.Log("[ERROR] CodeJoin: " + e);
            OnCodeJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void IdJoin(string p_lobbyId)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try {

            m_joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(p_lobbyId);

            string l_relayJoinCode = m_joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation l_joinAllocation = await JoinRelay(l_relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(l_joinAllocation, "dtls"));

            MultiplayerManager.Instance.StartClient();

        } catch (LobbyServiceException e) {
            Debug.Log("[ERROR] IdJoin: " + e);
            OnCodeJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void DeleteLobby()
    {
        if (m_joinedLobby != null)
        {
            try {

                await LobbyService.Instance.DeleteLobbyAsync(m_joinedLobby.Id);
                m_joinedLobby = null;

            } catch (LobbyServiceException e) {
                Debug.Log("[ERROR] DeleteLobby: " + e);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (m_joinedLobby != null)
        {
            try {

                await LobbyService.Instance.RemovePlayerAsync(m_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                m_joinedLobby = null;

            } catch (LobbyServiceException e) {
                Debug.Log("[ERROR] LeaveLobby: " + e);
            }
        }
    }

    public Lobby GetLobby()
    {
        return m_joinedLobby;
    }

}
