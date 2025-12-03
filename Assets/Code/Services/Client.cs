#if !UNITY_SERVER
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
#endif
using UnityEngine;

namespace Services
{
    public class Client : MonoBehaviour
    {
#if !UNITY_SERVER
        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
#endif
    }
}