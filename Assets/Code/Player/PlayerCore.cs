using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientAuthNetworkTransform))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerState))]
    public class PlayerCore : NetworkBehaviour
    {
        private PlayerMovement _playerMovement;
        private PlayerState _playerState;

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _playerState = GetComponent<PlayerState>();
        }
        
        public override void OnNetworkSpawn()
        {
            SyncTransform_ServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SyncTransform_ServerRpc()
        {
            UpdateTransform_ClientRpc(transform.position, transform.rotation);
        }

        [ClientRpc]
        public void UpdateTransform_ClientRpc(Vector3 position, Quaternion rotation)
        {
            if (!IsOwner) return;
            
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
