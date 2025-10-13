using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Archive.Movement.ServerAuth
{
    /*
     * An attempt at making simple client-side prediction.
     */
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public class ServerAuthMovementWithPrediction : NetworkBehaviour
    {
        [SerializeField] private float _movementSpeed = 10f;
        [SerializeField] private GameObject _localRepresentationPrefab;
        [SerializeField] private GameObject _networkRepresentationPrefab;
        private Transform _localRepresentationTransform;
        private Transform _networkRepresentationTransform;
        
        // Custom tick is necessary because deltaTime can be different for every machine
        private const int NetworkTickRate = 60;
        private float TickInterval => 1f / NetworkTickRate;
        private float _tickTimer;
        
        public override void OnNetworkSpawn()
        {
            InitRepresentationObjects();
        }

        private void Update()
        {
            if (!IsOwner) return;
            
            // Handle tick rate
            _tickTimer += Time.deltaTime;
            while (_tickTimer >= TickInterval)
            {
                Tick();
                _tickTimer -= TickInterval;
            }
        }
    
        private void Tick()
        {
            var input = Input.GetAxis("Horizontal");
            if (input == 0f) return;
            
            // Apply movement locally (doesn't affect other players)
            MoveLocalRepresentation(input);
            
            // Move on the server
            RequestToMove_ServerRpc(input);
        }

        private void MoveLocalRepresentation(float input)
        {
            // Horizontal movement (local representation only)
            _localRepresentationTransform.Translate(Vector3.right * input * TickInterval * _movementSpeed);
        }
        
        [ServerRpc]
        private void RequestToMove_ServerRpc(float input)
        {
            // Horizontal movement
            transform.Translate(Vector3.right * input * TickInterval * _movementSpeed);
        }

        private void InitRepresentationObjects()
        {
            if (_localRepresentationPrefab == null)
            {
                _localRepresentationPrefab = new GameObject("EmptyLocalRepresentation");
                Debug.LogError("Local representation prefab not specified.");
            }

            if (_networkRepresentationPrefab == null)
            {
                _networkRepresentationPrefab = new GameObject("EmptyNetworkRepresentation");
                Debug.LogError("Network representation prefab not specified.");
            }
            
            _localRepresentationTransform = Instantiate(_localRepresentationPrefab, transform.position, transform.rotation).transform;
            _networkRepresentationTransform = Instantiate(_networkRepresentationPrefab, transform.position, transform.rotation).transform;
            _networkRepresentationTransform.SetParent(transform);
            
            // Only show to the local representation to the local player (owner)
            _localRepresentationTransform.gameObject.SetActive(IsOwner);
            _networkRepresentationTransform.gameObject.SetActive(!IsOwner);
        }

        public override void OnDestroy()
        {
            // Clean up representation objects
            if (_localRepresentationTransform != null)
                Destroy(_localRepresentationTransform.gameObject);
            if (_networkRepresentationTransform != null)
                Destroy(_networkRepresentationTransform.gameObject);
        }
    }
}