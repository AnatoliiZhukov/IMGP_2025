using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /*
     * Taken from NGO Minimal Setup Scene.
     * Makes it possible to start host/client outside the editor.
     */
    public class MultiplayerButtons : MonoBehaviour
    {
        [SerializeField] private Button startHostButton;
        [SerializeField] private Button startClientButton;

        private void Start()
        {
            startHostButton.onClick.AddListener(StartHost);
            startClientButton.onClick.AddListener(StartClient);
        }

        private void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            ShowButtons(false);
        }

        private void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            ShowButtons(false);
        }

        private void ShowButtons(bool bShow = true)
        {
            startHostButton.transform.parent.gameObject.SetActive(bShow);
        }
    }
}
