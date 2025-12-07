using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    /*
     * Manages player connections etc.
     */
    public class GameSession : MonoBehaviour
    {
        //- Singleton -//
        public static GameSession Instance { get; private set; }
        private void CheckInstance()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        [field: SerializeField] public int MaxPlayers { get; private set; } = 2;
        
        [SerializeField] private NetworkObject _playerNetworkObject;
        
        private Transform[] _spawnLocations;

        private void Awake() {
            CheckInstance();
            DontDestroyOnLoad(gameObject);
        }

        #region NetworkManagerSetup
        private void InitHost() {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
            
            NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
            
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }

        private void InitClient() {
            NetworkManager.Singleton.StartClient();
        }

        private void InitShutdown() {
            SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            NetworkManager.Singleton.Shutdown();
        }

        private void OnPreShutdown() {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnPreShutdown -= OnPreShutdown;
            
            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApproval;

            if (NetworkManager.Singleton.SceneManager != null) {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
            
            _spawnLocations = null;
        }

        private void OnDestroy() {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer ) OnPreShutdown();
        }
        #endregion

        private void OnMaxPlayersReached()
        {
            Debug.Log("Ready to start the game");
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");

            if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers) {
                OnMaxPlayersReached();
            }
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
            if (sceneName != "GameScene") return;
            
            InitSpawnLocations();
            SpawnPlayerObjects();
        }
        
        private void InitSpawnLocations()
        {
            // Gather all Spawn Location objects
            var spawnLocationObjects = GameObject.FindGameObjectsWithTag("SpawnLocation");
            
            // If none are present, add the World Origin as the only spawn location
            if (spawnLocationObjects.Length == 0)
            {
                var go = Instantiate(new GameObject("SpawnLocation"), Vector3.zero, Quaternion.identity);
                _spawnLocations = new [] { go.transform };
                return;
            }
            
            // Convert existing Spawn Location objects into an array of Transforms
            _spawnLocations = new Transform[spawnLocationObjects.Length];
            for (int i = 0; i < spawnLocationObjects.Length; i++)
                _spawnLocations[i] = spawnLocationObjects[i].transform;
        }

        private void SpawnPlayerObjects() {
            // Check Player Prefab
            if (_playerNetworkObject == null) {
                Debug.LogError("Player Prefab is not assigned!");
                return;
            }
            
            var playerObjectIndex = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList) {
                // Cycle through spawn locations
                var spawnLocationIndex = playerObjectIndex % _spawnLocations.Length;
                var spawnPos = _spawnLocations[spawnLocationIndex].position;
                
                var playerObj = Instantiate(_playerNetworkObject, spawnPos, Quaternion.identity);
                playerObj.SpawnAsPlayerObject(client.ClientId, true);
                
                playerObjectIndex++;
            }
        }

        private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // Manual spawning
            response.CreatePlayerObject = false;
            
            if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers)
            {
                // Reject (Game full)
                response.Approved = false;
                response.Reason = "Game full";
                Debug.Log($"Rejected client {request.ClientNetworkId}: Game full ({NetworkManager.Singleton.ConnectedClients.Count}/{MaxPlayers})");
            }
            else
            {
                // Approve
                response.Approved = true;
                
                Debug.Log($"Approved client {request.ClientNetworkId}: {NetworkManager.Singleton.ConnectedClients.Count + 1}/{MaxPlayers} players");
            }
            
            // Instant response
            response.Pending = false;
        }
        
        #region NetworkButtons
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
        
                if (!NetworkManager.Singleton.IsListening)
                {
                    if (GUILayout.Button("Start Host", GUILayout.Height(ButtonHeight)))
                        InitHost();
                    
                    if (GUILayout.Button("Start Client", GUILayout.Height(ButtonHeight)))
                        InitClient();
                    
                    // if (GUILayout.Button("Start Server", GUILayout.Height(ButtonHeight)))
                    // {
                    //     NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
                    //     NetworkManager.Singleton.StartServer();
                    // }
                }
                else
                {
                    if (GUILayout.Button("Disconnect", GUILayout.Height(ButtonHeight))) {
                        InitShutdown();
                    }
                }
            }
        }
        #endregion
    }
}
