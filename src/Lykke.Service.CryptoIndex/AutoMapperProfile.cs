using AutoMapper;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Client.Models.Settings, Domain.Models.Settings>();
            CreateMap<Domain.Models.Settings, Client.Models.Settings>();

            CreateMap<IndexHistory, Client.Models.IndexHistory>();
            CreateMap<IndexHistory, Client.Models.PublicIndexHistory>();
        }
    }
}
