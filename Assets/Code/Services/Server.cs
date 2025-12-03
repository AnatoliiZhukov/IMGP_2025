#if UNITY_SERVER || ENABLE_UCS_SERVER
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication.Server;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
#endif
using System;
using UnityEngine;

namespace Services
{
    public class Server : MonoBehaviour
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
                    var session = m_SessionManager.Session;
                    await m_SessionManager.SetPlayerReadinessAsync(true);
                    Debug.Log("[Multiplay] Server is ready to accept players");
                    
                    NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }
            }
        }
#endif
    }
}
