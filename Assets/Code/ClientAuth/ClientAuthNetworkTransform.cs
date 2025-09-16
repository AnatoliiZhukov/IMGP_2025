using Unity.Netcode.Components;

namespace ClientAuth
{
    public class ClientAuthNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
