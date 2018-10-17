using AutoMapper;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.IndexHistory;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.IndexState;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.Settings;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.MarketCap;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexHistory;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexState;
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

            CreateMap<IndexHistory, IndexHistoryEntity>(MemberList.Source);
            CreateMap<IndexHistoryEntity, IndexHistory>();

            CreateMap<IndexState, IndexStateEntity>(MemberList.Source);
            CreateMap<IndexStateEntity, IndexState>();

            CreateMap<Settings, SettingsEntity>(MemberList.Source);
            CreateMap<SettingsEntity, Settings>();
        }
    }
}
