using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        private CharacterController _controller;
        
        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }
        
        public void MovePlayer()
        {
            if (!enabled) return;
            
            _controller.Move(Vector3.forward * Time.deltaTime);
        }
    }
}
