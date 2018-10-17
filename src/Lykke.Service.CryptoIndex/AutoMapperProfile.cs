using AutoMapper;

namespace Lykke.Service.CryptoIndex
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Client.Models.LCI10.Settings, Domain.LCI10.Settings.Settings>();
            CreateMap<Domain.LCI10.Settings.Settings, Client.Models.LCI10.Settings>();

            CreateMap<Domain.LCI10.IndexHistory.IndexHistory, Client.Models.LCI10.IndexHistory>();
        }
    }
}
