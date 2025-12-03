#if !UNITY_SERVER
using System;
using System.Threading;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine.UI;
#endif
using UnityEngine;

namespace Services
{
    public class MatchmakingButtons : MonoBehaviour
    {
#if !UNITY_SERVER
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _cancelButton;
    
        private CancellationTokenSource _matchmakingCts;
    
        private void Awake()
        {
            _startButton.onClick.AddListener(() =>
            {
                ToggleButtons();
                BeginMatchmaking();
            });
        
            _cancelButton.onClick.AddListener(() =>
            {
                ToggleButtons();
                _matchmakingCts?.Cancel();
            });  
        }

        private async void BeginMatchmaking()
        {
            try
            {
                var matchmakerOptions = new MatchmakerOptions
                {
                    QueueName = "Default"
                };

                var sessionOptions = new SessionOptions { MaxPlayers = 2 }.WithDirectNetwork();
                
                _matchmakingCts = new CancellationTokenSource();

                var session = await MultiplayerService.Instance.MatchmakeSessionAsync(matchmakerOptions, sessionOptions, _matchmakingCts.Token);
                Debug.Log("Joining session...");
                
                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void ToggleButtons()
        {
            _startButton.gameObject.SetActive(!_startButton.gameObject.activeSelf);
            _cancelButton.gameObject.SetActive(!_cancelButton.gameObject.activeSelf);
        }
#endif
    }
}
