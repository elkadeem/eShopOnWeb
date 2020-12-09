using BlazorAdmin.Services;
using BlazorApplicationInsights;
using BlazorShared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace BlazorAdmin
{
    public static class ServicesConfiguration
    {
        public static IServiceCollection AddBlazorServices(this IServiceCollection services)
        {
            services.AddScoped<ICatalogBrandService, CachedCatalogBrandServiceDecorator>();
            services.AddScoped<CatalogBrandService>();
            services.AddScoped<ICatalogTypeService, CachedCatalogTypeServiceDecorator>();
            services.AddScoped<CatalogTypeService>();
            services.AddScoped<ICatalogItemService, CachedCatalogItemServiceDecorator>();
            services.AddScoped<CatalogItemService>();

            //Application Insights
            services.AddBlazorApplicationInsights(async applicationInsights =>
            {
                var telemetryItem = new TelemetryItem()
                {                    
                    Tags = new Dictionary<string, object>()
            {
                { "ai.cloud.role", "SPA" },
                { "ai.cloud.roleInstance", "Blazor Wasm" },
            }
                };

                await applicationInsights.AddTelemetryInitializer(telemetryItem);
            });

            return services;
        }
    }
}
