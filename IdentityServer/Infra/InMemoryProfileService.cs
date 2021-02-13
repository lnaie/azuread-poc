
using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;

namespace IdentityServer.Infra
{
    public class InMemoryProfileService : IProfileService
    {
        public InMemoryProfileService()
        {
        }

        /// <inheritdoc/>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            TryAddProfileClaims(context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }

        private void TryAddProfileClaims(ProfileDataRequestContext context)
        {
            var requestesIdenityResource = context.RequestedResources.Resources.IdentityResources;
            var requestedClaimTypes = context.RequestedClaimTypes;
            var user = context.Subject;

            if (user == null)
            {
                return;
            }

            var hasRequestedProfileClaims = (requestedClaimTypes != null && requestedClaimTypes.Contains("profile"))
                || (requestesIdenityResource != null && requestesIdenityResource.FirstOrDefault(x => x.Name == "profile") != null);
            if (hasRequestedProfileClaims)
            {
                foreach (var claim in user.Claims)
                {
                    switch (claim.Type)
                    {
                        case JwtClaimTypes.Name:
                        case JwtClaimTypes.Email:
                        case JwtClaimTypes.EmailVerified:
                        case JwtClaimTypes.Role:
                        //case JwtClaimTypes.GivenName:
                        //case JwtClaimTypes.FamilyName:
                        //case JwtClaimTypes.WebSite:
                        //case JwtClaimTypes.Address:
                            context.IssuedClaims.Add(claim);
                            break;
                    }
                }
            }
        }
    }
}
