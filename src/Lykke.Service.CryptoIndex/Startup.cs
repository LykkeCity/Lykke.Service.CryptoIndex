using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.CryptoIndex.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;

namespace Lykke.Service.CryptoIndex
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "CryptoIndex API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();

            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "CryptoIndexLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.CryptoIndexService.Db.LogsConnectionString;
                };

                Mapper.Initialize(cfg =>
                {
                    cfg.AddProfiles(typeof(Domain.Repositories.AutoMapperProfile));
                    cfg.AddProfiles(typeof(AutoMapperProfile));
                });
                Mapper.AssertConfigurationIsValid();
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                app.UseResponseCaching();

                options.SwaggerOptions = _swaggerOptions;
                options.DefaultErrorHandler = exception => ErrorResponse.Create(exception.Message);
            });
        }
    }
}
