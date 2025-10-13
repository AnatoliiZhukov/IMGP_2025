using Unity.Netcode.Components;
using UnityEngine;

namespace Player
{
    /*
     * Allows to directly modify Transform as a client with no server validation.
     * Can be used to make player movement feel more responsive when playing with latency.
     * No validation = it's easy to cheat.
     */
    public class ClientAuthNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
