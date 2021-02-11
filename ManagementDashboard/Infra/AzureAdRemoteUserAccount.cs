using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace ManagementDashboard.Infra
{
    /// <summary>
    /// Custom Azure AD user account.
    /// </summary>
    /// <remarks>
    /// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/azure-active-directory-groups-and-roles?view=aspnetcore-5.0#custom-user-account
    /// </remarks>
    public class AzureAdRemoteUserAccount : RemoteUserAccount
    {
        /// <summary>
        /// AAD App Roles array.
        /// </summary>
        [JsonPropertyName("roles")]
        public string[] Roles { get; set; } = Array.Empty<string>();

        /// <summary>
        /// AAD Administrator Roles in well-known IDs claims (wids).
        /// </summary>
        [JsonPropertyName("wids")]
        public string[] Wids { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Immutable object identifier claim.
        /// </summary>
        [JsonPropertyName("oid")]
        public string Oid { get; set; }
    }
}
