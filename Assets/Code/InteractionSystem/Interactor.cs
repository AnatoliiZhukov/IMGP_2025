using Unity.Netcode;
using UnityEngine;

namespace InteractionSystem
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class Interactor : NetworkBehaviour
    {
        private Rigidbody _rb;
        private SphereCollider _interactorCollider;
        private IInteractable _interactableInRange;
    
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
        
            _interactorCollider = GetComponent<SphereCollider>();
            _interactorCollider.isTrigger = true;
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                RequestInteraction_ServerRpc();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer)
            {
                var interactable = other.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    _interactableInRange = interactable;
                    Debug.Log("Interactable in range updated");
                }
            }
        }

        [ServerRpc]
        private void RequestInteraction_ServerRpc()
        {
            _interactableInRange.Interact(this);
        }
    }
}
