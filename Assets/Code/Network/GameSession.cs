using Player;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    /*
     * Manages player connections etc.
     */
    [RequireComponent(typeof(NetworkObject))]
    public class GameSession : NetworkBehaviour
    {
        [field: SerializeField] public int MaxPlayers { get; private set; } = 2;
        
        [SerializeField] private NetworkObject _playerNetworkObject;
        
        private Transform[] _spawnLocations;

        private void InitSpawnLocations()
        {
            // Gather all Spawn Location objects
            var spawnLocationObjects = GameObject.FindGameObjectsWithTag("SpawnLocation");
            
            // If none are present, add this object's Transform as the only Spawn Location
            if (spawnLocationObjects.Length == 0)
            {
                _spawnLocations = new [] { gameObject.transform };
                return;
            }
            
            // Convert existing Spawn Location objects into an array of Transforms
            _spawnLocations = new Transform[spawnLocationObjects.Length];
            for (int i = 0; i < spawnLocationObjects.Length; i++)
                _spawnLocations[i] = spawnLocationObjects[i].transform;
        }
        
        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton == null || !IsServer) return;

            InitSpawnLocations();
            
            // Can be used to process new connections/disconnects
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton == null || !IsServer) return;
            
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            
            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApproval;
        }
        
        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");
            
            // Cycle through spawn locations if there aren't enough
            var spawnLocationIndex = (NetworkManager.Singleton.ConnectedClients.Count - 1) % _spawnLocations.Length;
            
            // Check Player Prefab
            if (!_playerNetworkObject)
            {
                Debug.LogError("Player Prefab is not assigned!");
                return;
            }
            
            // Init Player
            var spawnTransform = _spawnLocations[spawnLocationIndex];
            var playerNetworkObject = Instantiate(_playerNetworkObject, spawnTransform.position, spawnTransform.rotation);
            playerNetworkObject.SpawnWithOwnership(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
        }
        
        private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers)
            {
                // Reject (Game full)
                response.Approved = false;
                response.CreatePlayerObject = false;
                response.Reason = "Game full";
                Debug.Log($"Rejected client {request.ClientNetworkId}: Game full ({NetworkManager.Singleton.ConnectedClients.Count}/{MaxPlayers})");
            }
            else
            {
                // Approve
                response.Approved = true;
                response.CreatePlayerObject = true;
            
                Debug.Log($"Approved client {request.ClientNetworkId}: {NetworkManager.Singleton.ConnectedClients.Count + 1}/{MaxPlayers} players");
            }
            
            // Instant response
            response.Pending = false;
        }
        
        // Draw network buttons
        private const float GUIWidth = 200f;
        private const float GUIHeight = 300f;
        private const float GUIPadding = 20f;
        private const float ButtonHeight = 30f;
        private void OnGUI()
        {
            if (NetworkManager.Singleton == null) return;

            using (new GUILayout.AreaScope(new Rect(GUIPadding, GUIPadding, GUIWidth, GUIHeight)))
            {
                GUILayout.Label($"Players: {NetworkManager.Singleton.ConnectedClients.Count}/{MaxPlayers}");
        
                if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
                {
                    if (GUILayout.Button("Start Host", GUILayout.Height(ButtonHeight)))
                    {
                        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
                        NetworkManager.Singleton.StartHost();
                    }
                    if (GUILayout.Button("Start Client", GUILayout.Height(ButtonHeight)))
                        NetworkManager.Singleton.StartClient();
                    
                    if (GUILayout.Button("Start Server", GUILayout.Height(ButtonHeight)))
                    {
                        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
                        NetworkManager.Singleton.StartServer();
                    }
                }
                else
                {
                    if (GUILayout.Button("Disconnect", GUILayout.Height(ButtonHeight)))
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                }
            }
        }
    }
}
