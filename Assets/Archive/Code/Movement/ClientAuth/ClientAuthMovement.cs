using Player;
using Unity.Netcode;
using UnityEngine;

namespace Archive.Movement.ClientAuth
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientAuthNetworkTransform))]
    public class ClientAuthMovement : NetworkBehaviour
    {
        [SerializeField] private float _movementSpeed = 10f;
        private void Update()
        {
            // Do nothing if we have no ownership
            // In this case it's needed so every client can only move their PlayerPrefab
            if (!IsOwner) return;
        
            // Horizontal movement
            if (Input.GetAxis("Horizontal") == 0) return;
            var T = transform;
            T.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * _movementSpeed);
        }
    }
}
