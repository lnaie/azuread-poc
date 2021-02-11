using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ManagementDashboard.Infra;
using ManagementDashboard.Services;
using System.Net.Http;

namespace ManagementDashboard
{
    /// <summary>
    /// App entry point.
    /// </summary>
    /// <remarks>
    /// App configuration with AAD: 
    /// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-azure-active-directory?view=aspnetcore-5.0
    /// </remarks>
    public class AppProgram
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddOptions();

            builder.Services.AddMsalAuthentication<RemoteAuthenticationState, AzureAdRemoteUserAccount>(options =>
                {
                    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                    // The default option, that is a popup, doesn't always redirect properly
                    options.ProviderOptions.LoginMode = "redirect";

                    options.ProviderOptions.AdditionalScopesToConsent.Add($"api://{options.ProviderOptions.Authentication.ClientId}/api.access");

                    // 'appRole' claims are created in AzureAdAccountClaimsPrincipalFactory from 'roles' claim coming from AAD
                    options.UserOptions.RoleClaim = "appRole";
                })
                .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, AzureAdRemoteUserAccount, AzureAdAccountClaimsPrincipalFactory>();

            // TODOs service
            builder.Services.AddScoped<ApiAuthorizationMessageHandler>();
            builder.Services.AddHttpClient("IdentityApi", (sp, client) =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["IdentityApiUrl"]);
                    client.DefaultRequestHeaders.Add("Accept", new[] { "application/json" });
                    client.DefaultRequestHeaders.Add("User-Agent", new[] { Constants.ApiUserAgent });
                })
                .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();
            builder.Services.AddScoped<IdentityApiService>();

            // Weather service
            builder.Services.AddScoped(sp => new HttpClient 
            { 
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            // Start
            await builder.Build()
                .RunAsync();
        }
    }
}
