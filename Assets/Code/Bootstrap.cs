using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string _serverSceneName = "Server";
    [SerializeField] private string _clientSceneName = "Client";
    private void Start()
    {
#if UNITY_SERVER
        SceneManager.LoadScene(_serverSceneName, LoadSceneMode.Additive);
#else
        SceneManager.LoadScene(_clientSceneName, LoadSceneMode.Additive);
#endif
    }
}