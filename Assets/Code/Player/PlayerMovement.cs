using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float movementSpeed = 10f;
        private void Update()
        {
            if (!IsOwner) return;
            
            RequestToMove_ServerRpc(Input.GetAxis("Horizontal"));
        }

        [ServerRpc]
        private void RequestToMove_ServerRpc(float input)
        {
            // Horizontal movement
            var T = transform;
            T.Translate(Vector3.right * input * Time.deltaTime * movementSpeed);
        }
    }
}
