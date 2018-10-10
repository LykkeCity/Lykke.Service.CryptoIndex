using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Client
{
    /// <summary>
    /// CryptoIndex client interface.
    /// </summary>
    [PublicAPI]
    public interface ICryptoIndexClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - ICryptoIndexApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        ICryptoIndexApi Api { get; }
    }
}
