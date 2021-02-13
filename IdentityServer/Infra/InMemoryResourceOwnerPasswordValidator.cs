
using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;

namespace IdentityServer.Infra
{
    public class InMemoryResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public InMemoryResourceOwnerPasswordValidator()
        {
        }

        /// <inheritdoc/>
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var userEntity = TestUsers.GetUsers()
                .FirstOrDefault(x => x.Username == context.UserName && x.Password == context.Password);

            if (userEntity != null)
            {
                context.Result = new GrantValidationResult(
                    subject: userEntity.SubjectId,
                    authenticationMethod: context.Request.GrantType, //"custom",
                    claims: userEntity.Claims);
                return;
            }

            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "invalid user credentials");
        }
    }
}
