using BlazorAdmin.Services;
using BlazorApplicationInsights;
using Blazored.LocalStorage;
using BlazorShared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorAdmin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("admin");

            var baseUrlConfig = new BaseUrlConfiguration();
            builder.Configuration.Bind(BaseUrlConfiguration.CONFIG_NAME, baseUrlConfig);
            builder.Services.AddScoped<BaseUrlConfiguration>(sp => baseUrlConfig);

            builder.Services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //Application Insights
            builder.Services.AddBlazorApplicationInsights(async applicationInsights =>
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

            builder.Services.AddScoped<HttpService>();

            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped(sp => (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

            builder.Services.AddBlazorServices();


            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

            

            await ClearLocalStorageCache(builder.Services);

            builder.Build().RunAsync();
        }

        private static async Task ClearLocalStorageCache(IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            var localStorageService = sp.GetRequiredService<ILocalStorageService>();

            await localStorageService.RemoveItemAsync("brands");
        }
    }
}
