using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerState : NetworkBehaviour
    {
        public NetworkVariable<int> _health = new NetworkVariable<int>
            (10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [SerializeField] private HealthDisplay _healthDisplayPrefab;
        [SerializeField] private Vector3 _healthDisplayOffset = new Vector3(0f, 3f, 0f);
        private HealthDisplay _healthDisplay;

        // public struct CustomValueType : INetworkSerializable
        // {
        //     public bool SomeBool;
        //     public int SomeInt;
        //     public FixedString32Bytes SomeString; // 32 symbols long
        //     
        //     // Serialization instructions (required)
        //     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        //     {
        //         serializer.SerializeValue(ref SomeBool);
        //         serializer.SerializeValue(ref SomeInt);
        //         serializer.SerializeValue(ref SomeString);
        //     }
        // }
        
        private void Update()
        {
            if (!IsOwner) return;
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _health.Value--;
            }
        }

        public override void OnNetworkSpawn()
        {
            _health.OnValueChanged += OnHealthChanged;
            
            InitHealthDisplay();
        }

        private void OnHealthChanged(int oldValue, int newValue)
        {
            _healthDisplay.UpdateHealth(newValue);
        }

        private void InitHealthDisplay()
        {
            _healthDisplay = Instantiate(_healthDisplayPrefab, transform.position + _healthDisplayOffset, Quaternion.identity);
            _healthDisplay.transform.SetParent(transform);
        }
    }
}
