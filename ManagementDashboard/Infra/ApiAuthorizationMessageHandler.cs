using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace ManagementDashboard.Infra
{
    /// <summary>
    /// Defines the authorized urls and scopes that are used to request a token from AAD to access the APIs.
    /// </summary>
    public class ApiAuthorizationMessageHandler : ManagementDashboard.Infra.AuthorizationMessageHandler
    {
        public ApiAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigationManager, IConfiguration configuration)
            : base(provider, navigationManager)
        {
            var authenticationScheme =  "AzureADBearer"; //"Bearer";
            var appId = configuration["AzureAd:ClientId"];

            ConfigureHandler(
                authorizedUrls: new[]
                {
                    navigationManager.BaseUri,
                    configuration["IdentityApiUrl"]
                },
                scopes: new[]
                {
                    "openid",
                    // This one scope will push the roles into the jwt access token requested for API access
                    $"api://{appId}/api.access",
                },
                authenticationScheme: authenticationScheme);
        }
    }

    // NOTE: This is a modification of
    // https://github.com/dotnet/aspnetcore/blob/main/src/Components/WebAssembly/WebAssembly.Authentication/src/Services/AuthorizationMessageHandler.cs
    // to support configurable authentication scheme.
    //
    /// <summary>
    /// A <see cref="DelegatingHandler"/> that attaches access tokens to outgoing <see cref="HttpResponseMessage"/> instances.
    /// Access tokens will only be added when the request URI is within one of the base addresses configured using
    /// <see cref="ConfigureHandler(IEnumerable{string}, IEnumerable{string}, string)"/>.
    /// </summary>
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider _provider;
        private readonly NavigationManager _navigation;
        private AccessToken _lastToken;
        private AuthenticationHeaderValue _cachedHeader;
        private Uri[] _authorizedUris;
        private AccessTokenRequestOptions _tokenOptions;
        //NOTE: Changed from original implementation with a configurable scheme
        private string _authenticationScheme = "Bearer";

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorizationMessageHandler"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IAccessTokenProvider"/> to use for provisioning tokens.</param>
        /// <param name="navigation">The <see cref="NavigationManager"/> to use for performing redirections.</param>
        public AuthorizationMessageHandler(
            IAccessTokenProvider provider,
            NavigationManager navigation)
        {
            _provider = provider;
            _navigation = navigation;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;
            if (_authorizedUris == null)
            {
                throw new InvalidOperationException($"The '{nameof(AuthorizationMessageHandler)}' is not configured. " +
                    $"Call '{nameof(AuthorizationMessageHandler.ConfigureHandler)}' and provide a list of endpoint urls to attach the token to.");
            }

            if (_authorizedUris.Any(uri => uri.IsBaseOf(request.RequestUri)))
            {
                if (_lastToken == null || now >= _lastToken.Expires.AddMinutes(-5))
                {
                    var tokenResult = _tokenOptions != null ?
                        await _provider.RequestAccessToken(_tokenOptions) :
                        await _provider.RequestAccessToken();

                    if (tokenResult.TryGetToken(out var token))
                    {
                        _lastToken = token;
                        _cachedHeader = new AuthenticationHeaderValue(_authenticationScheme, _lastToken.Value);
                    }
                    else
                    {
                        throw new AccessTokenNotAvailableException(_navigation, tokenResult, _tokenOptions?.Scopes);
                    }
                }

                // We don't try to handle 401s and retry the request with a new token automatically since that would mean we need to copy the request
                // headers and buffer the body and we expect that the user instead handles the 401s. (Also, we can't really handle all 401s as we might
                // not be able to provision a token without user interaction).
                request.Headers.Authorization = _cachedHeader;
            }

            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Configures this handler to authorize outbound HTTP requests using an access token. The access token is only attached if at least one of
        /// <paramref name="authorizedUrls" /> is a base of <see cref="HttpRequestMessage.RequestUri" />.
        /// </summary>
        /// <param name="authorizedUrls">The base addresses of endpoint URLs to which the token will be attached.</param>
        /// <param name="scopes">The list of scopes to use when requesting an access token.</param>
        /// <param name="returnUrl">The return URL to use in case there is an issue provisioning the token and a redirection to the
        /// identity provider is necessary.
        /// </param>
        /// <param name="authenticationScheme">
        /// The authentication scheme to be used with in Authorization, ProxyAuthorization, WWW-Authenticate, and Proxy-Authenticate headers.
        /// Defaults to 'Bearer'.
        /// </param>
        /// <returns>This <see cref="AuthorizationMessageHandler"/>.</returns>
        public AuthorizationMessageHandler ConfigureHandler(
            IEnumerable<string> authorizedUrls,
            IEnumerable<string> scopes = null,
            string returnUrl = null,
            string authenticationScheme = null)
        {
            if (_authorizedUris != null)
            {
                throw new InvalidOperationException("Handler already configured.");
            }

            if (authorizedUrls == null)
            {
                throw new ArgumentNullException(nameof(authorizedUrls));
            }

            var uris = authorizedUrls.Select(uri => new Uri(uri, UriKind.Absolute)).ToArray();
            if (uris.Length == 0)
            {
                throw new ArgumentException("At least one URL must be configured.", nameof(authorizedUrls));
            }

            _authorizedUris = uris;
            var scopesList = scopes?.ToArray();
            if (scopesList != null || returnUrl != null)
            {
                _tokenOptions = new AccessTokenRequestOptions
                {
                    Scopes = scopesList,
                    ReturnUrl = returnUrl
                };
            }

            if (!string.IsNullOrWhiteSpace(authenticationScheme))
            {
                _authenticationScheme = authenticationScheme;
            }

            return this;
        }
    }
}
