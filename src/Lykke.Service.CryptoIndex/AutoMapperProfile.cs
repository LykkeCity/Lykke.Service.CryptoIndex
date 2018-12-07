﻿using System.Linq;
using AutoMapper;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Client.Models.Settings, Domain.Models.Settings>();

            CreateMap<Domain.Models.Settings, Client.Models.Settings>()
                .ForMember(dest => dest.FrozenAssets, opt =>
                    opt.MapFrom(src => src.AssetsSettings.Where(x => x.IsDisabled)));

            CreateMap<IndexHistory, Client.Models.IndexHistory>()
                .ForMember(dest => dest.FrozenAssets, opt =>
                    opt.MapFrom(src => src.AssetsSettings.Where(x => x.IsDisabled)));

            CreateMap<IndexHistory, Client.Models.PublicIndexHistory>()
                .ForMember(dest => dest.FrozenAssets, opt =>
                    opt.MapFrom(src => src.AssetsSettings.Where(x => x.IsDisabled)));
        }
    }
}
