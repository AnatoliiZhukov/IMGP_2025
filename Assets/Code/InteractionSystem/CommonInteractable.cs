using UnityEngine;

namespace InteractionSystem
{
    public interface IInteractable
    {
        void Interact(Interactor interactor);
    }
    
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class CommonInteractable : MonoBehaviour, IInteractable
    {
        private Rigidbody _rb;
        private SphereCollider _interactableCollider;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            
            _interactableCollider = GetComponent<SphereCollider>();
            _interactableCollider.isTrigger = true;
        }

        public void Interact(Interactor interactor)
        {
            Debug.Log(interactor.gameObject.name + " is interacting");
        }
    }
}
