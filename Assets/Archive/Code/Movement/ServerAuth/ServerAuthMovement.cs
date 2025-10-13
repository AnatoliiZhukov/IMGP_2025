using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Archive.Movement.ServerAuth
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public class ServerAuthMovement : NetworkBehaviour
    {
        [SerializeField] private float _movementSpeed = 10f;
        private void Update()
        {
            if (!IsOwner) return;
            
            // Send input, apply on the server
            RequestToMove_ServerRpc(Input.GetAxis("Horizontal"));
        }
        
        [ServerRpc]
        private void RequestToMove_ServerRpc(float input)
        {
            // Horizontal movement
            var T = transform;
            T.Translate(Vector3.right * input * Time.deltaTime * _movementSpeed);
        }
    }
}
