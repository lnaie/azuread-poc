using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Extensions.Logging;

namespace ManagementDashboard.Infra
{
    /// <summary>
    /// Custom Azure AD claims principal factory.
    /// </summary>
    /// <remarks>
    /// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/azure-active-directory-groups-and-roles?view=aspnetcore-5.0#custom-user-account
    /// </remarks>
    public class AzureAdAccountClaimsPrincipalFactory : AccountClaimsPrincipalFactory<AzureAdRemoteUserAccount>
    {
        private readonly ILogger<AzureAdAccountClaimsPrincipalFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AzureAdAccountClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor, IServiceProvider serviceProvider, ILogger<AzureAdAccountClaimsPrincipalFactory> logger)
            : base(accessor)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async override ValueTask<ClaimsPrincipal> CreateUserAsync(AzureAdRemoteUserAccount account, RemoteAuthenticationUserOptions options)
        {
            var initialUser = await base.CreateUserAsync(account, options);

            if (initialUser.Identity.IsAuthenticated)
            {
                var userIdentity = initialUser.Identity as ClaimsIdentity;

                if (userIdentity == null)
                {
                    return initialUser;
                }

                foreach (var role in account.Roles)
                {
                    userIdentity.AddClaim(new Claim("appRole", role));
                }

                foreach (var wid in account.Wids)
                {
                    userIdentity.AddClaim(new Claim("directoryRole", wid));
                }
            }

            return initialUser;
        }
    }
}
