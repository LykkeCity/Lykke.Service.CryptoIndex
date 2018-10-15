using AutoMapper;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.IndexSnapshot;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.Settings;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.MarketCap;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexSnapshot;
using Lykke.Service.CryptoIndex.Domain.LCI10.Settings;
using Lykke.Service.CryptoIndex.Domain.MarketCapitalization;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MarketCapitalization.MarketCap, MarketCapEntity>();
            CreateMap<MarketCapEntity, MarketCapitalization.MarketCap>();

            CreateMap<AssetMarketCap, AssetMarketCapEntity>();
            CreateMap<AssetMarketCapEntity, AssetMarketCap>();

            CreateMap<IndexSnapshot, IndexSnapshotEntity>(MemberList.Source);
            CreateMap<IndexSnapshotEntity, IndexSnapshot>();

            CreateMap<Settings, SettingsEntity>(MemberList.Source);
            CreateMap<SettingsEntity, Settings>();
        }
    }
}
