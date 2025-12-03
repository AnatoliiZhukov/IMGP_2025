#if UNITY_SERVER || ENABLE_UCS_SERVER
using System.Threading.Tasks;
using Unity.Services.Authentication.Server;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using System;
using UnityEngine;
#endif
using Unity.Netcode;

namespace Services
{
    public class Server : NetworkBehaviour
    {
#if UNITY_SERVER || ENABLE_UCS_SERVER

        // Server Query Protocol
        private const ushort k_DefaultMaxPlayers = 2;
        private const string k_DefaultServerName = "DefaultServerName";
        private const string k_DefaultGameType = "DefaultGameType";
        private const string k_DefaultBuildId = "159742";
        private const string k_DefaultMap = "DefaultMap";

        IMultiplaySessionManager m_SessionManager;

        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await ConnectToMultiplay();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private async Task ConnectToMultiplay()
        {
            if (UnityServices.Instance.GetMultiplayerService() != null)
            {
                // Authenticate
                await ServerAuthenticationService.Instance.SignInFromServerAsync();
                var token = ServerAuthenticationService.Instance.AccessToken;

                // Callbacks should be used to ensure proper state of the server allocation.
                // Awaiting the StartMultiplaySessionManagerAsync won't guarantee proper state.
                var callbacks = new MultiplaySessionManagerEventCallbacks();
                callbacks.Allocated += OnServerAllocatedCallback;

                var sessionManagerOptions = new MultiplaySessionManagerOptions()
                {
                    SessionOptions = new SessionOptions()
                    {
                        MaxPlayers = k_DefaultMaxPlayers
                    }.WithDirectNetwork(),

                    // Server options are REQUIRED for the underlying SQP server
                    MultiplayServerOptions = new MultiplayServerOptions(
                        serverName: k_DefaultServerName,
                        gameType: k_DefaultGameType,
                        buildId: k_DefaultBuildId,
                        map: k_DefaultMap,
                        autoReady: false
                    ),
                    Callbacks = callbacks
                };
                m_SessionManager = await MultiplayerServerService.Instance.StartMultiplaySessionManagerAsync(sessionManagerOptions);

               // Ensure that the session is only accessed after the allocation happened.
               // Otherwise you risk the Session being in an uninitialized state.
                async void OnServerAllocatedCallback(IMultiplayAllocation obj)
                {
                    
                    // var serverConfig = MultiplayService.Instance.ServerConfig;
                    //
                    // Debug.Log($"Server IP: {serverConfig.IpAddress}");
                    // Debug.Log($"Server Port: {serverConfig.Port}");
                    // Debug.Log($"Query Port: {serverConfig.QueryPort}");
                    // Debug.Log($"Allocation ID: {serverConfig.AllocationId}");
                    //
                    // NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(serverConfig.IpAddress, serverConfig.Port, "0.0.0.0");
                    
                    var session = m_SessionManager.Session;
                    
                    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                    NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
                    
                    NetworkManager.Singleton.StartServer();
                    NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    
                    await m_SessionManager.SetPlayerReadinessAsync(true);
                    Debug.Log("[Multiplay] Server is ready to accept players");
                }
            }
        }

        public override void OnDestroy()
        {
            if (NetworkManager.Singleton == null || !IsServer) return;
            
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            
            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApproval;
        }
        
        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
        }
        
        private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
            {
                // Reject (Game full)
                response.Approved = false;
                response.CreatePlayerObject = false;
                response.Reason = "Game full";
                Debug.Log($"Rejected client {request.ClientNetworkId}: Game full ({NetworkManager.Singleton.ConnectedClients.Count}/{2})");
            }
            else
            {
                // Approve
                response.Approved = true;
                response.CreatePlayerObject = true;
            
                Debug.Log($"Approved client {request.ClientNetworkId}: {NetworkManager.Singleton.ConnectedClients.Count + 1}/{2} players");
            }
            
            // Instant response
            response.Pending = false;
        }
#endif
    }
}
