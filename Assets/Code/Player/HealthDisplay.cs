using TMPro;
using UnityEngine;

namespace Player
{
    public class HealthDisplay : MonoBehaviour
    {
        private TMP_Text _healthText;

        private void Awake()
        {
            _healthText = transform.GetChild(0).GetComponent<TMP_Text>();
        }
        
        public void UpdateHealth(int newHealth)
        {
            _healthText.text = newHealth.ToString();
        }
    }
}
