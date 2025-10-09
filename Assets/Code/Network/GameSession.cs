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
        
        [SerializeField] private GameObject _playerPrefab;
        
        private Transform[] _spawnLocations;

        private void InitSpawnLocations()
        {
            // Gather all Spawn Location objects and convert them into an array of Transforms
            var spawnLocationObjects = GameObject.FindGameObjectsWithTag("SpawnLocation");
            _spawnLocations = new Transform[spawnLocationObjects.Length];
            for (int i = 0; i < spawnLocationObjects.Length; i++)
                _spawnLocations[i] = spawnLocationObjects[i].transform;
        }
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            InitSpawnLocations();
            
            // Can be used to process new connections/disconnects
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            
            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApproval;
        }
        
        private void OnClientConnected(ulong clientID)
        {
            // Cycle through spawn locations if there aren't enough
            var spawnLocationIndex = (NetworkManager.Singleton.ConnectedClients.Count - 1) % _spawnLocations.Length;
            
            // Init player prefab
            var playerNetworkObject = Instantiate(_playerPrefab, _spawnLocations[spawnLocationIndex]).GetComponent<NetworkObject>();
            playerNetworkObject.SpawnWithOwnership(clientID);
            
            Debug.Log($"Client {clientID} connected");
        }

        private void OnClientDisconnected(ulong clientID)
        {
            Debug.Log($"Client {clientID} disconnected");
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
        private void OnGUI()
        {
            // Don't draw under certain conditions
            if (NetworkManager.Singleton == null
                || NetworkManager.Singleton.IsClient
                || NetworkManager.Singleton.IsServer) 
                return;
            
            using (new GUILayout.AreaScope(new Rect(20f, 20f, 100f, 200f)))
            {
                if (GUILayout.Button("Start Host"))
                {
                    // Define Connection Approval callback
                    // Is used to limit max players in this example
                    // ("Connection Approval" needs to be set to "true" in the NetworkManager)
                    NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
                    
                    NetworkManager.Singleton.StartHost();
                }
                if (GUILayout.Button("Start Client"))
                    NetworkManager.Singleton.StartClient();
            }
        }
    }
}
