using AutoMapper;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;

namespace Lykke.Service.CryptoIndex
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Client.Models.LCI10.Settings, Domain.Models.LCI10.Settings>();
            CreateMap<Domain.Models.LCI10.Settings, Client.Models.LCI10.Settings>();

            CreateMap<IndexHistory, Client.Models.LCI10.IndexHistory>();
        }
    }
}
