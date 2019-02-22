using AutoMapper;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MarketCap, MarketCapEntity>();
            CreateMap<MarketCapEntity, MarketCap>();

            CreateMap<AssetMarketCap, AssetMarketCapEntity>();
            CreateMap<AssetMarketCapEntity, AssetMarketCap>();

            CreateMap<IndexHistory, IndexHistoryEntity>(MemberList.None);
            CreateMap<IndexHistory, IndexHistoryBlob>(MemberList.None);

            CreateMap<IndexState, IndexStateEntity>(MemberList.Source);
            CreateMap<IndexStateEntity, IndexState>();

            CreateMap<Warning, WarningEntity>(MemberList.Source);
            CreateMap<WarningEntity, Warning>();

            CreateMap<Settings, SettingsEntity>(MemberList.Source);
            CreateMap<SettingsEntity, Settings>();

            CreateMap<TickPrice, TickPriceEntity>(MemberList.Source);
            CreateMap<TickPriceEntity, TickPrice>();

            CreateMap<AssetPrice, AssetPriceEntity>(MemberList.Source);
            CreateMap<AssetPriceEntity, AssetPrice>();
        }
    }
}
