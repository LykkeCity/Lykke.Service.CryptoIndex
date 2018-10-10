using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CryptoIndex.Client 
{
    /// <summary>
    /// CryptoIndex client settings.
    /// </summary>
    public class CryptoIndexServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
