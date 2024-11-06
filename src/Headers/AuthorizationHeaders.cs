using System;
using System.Collections.Generic;

namespace AuthorizationInterceptor.Extensions.Abstractions.Headers
{
    /// <summary>
    /// Represents the headers returned by an authorization interceptor process or authentication method and including expiration information.
    /// </summary>
    public class AuthorizationHeaders : Dictionary<string, string>
    {
        /// <summary>
        /// Get auth headers in an OAuth settings
        /// </summary>
        public OAuthHeaders? OAuthHeaders { get; private set; }

        /// <summary>
        /// Gets the optional expiration timespan for the authorization data.
        /// </summary>
        /// <value>
        /// The duration for which the authorization data is considered valid, or <c>null</c> if it does not expire.
        /// </value>
        public TimeSpan? ExpiresIn { get; private set; }

        /// <summary>
        /// Gets the time at which the authorization data was generated/authenticated.
        /// </summary>
        /// <value>
        /// The <see cref="DateTimeOffset"/> representing the point in time the authorization data was generated/authenticated.
        /// </value>
        public DateTimeOffset AuthenticatedAt { get; private set; }

        /// <summary>
        /// Calculate the real expiration time
        /// </summary>
        /// <returns>Returns the real expiration time relative to now</returns>
        public TimeSpan? GetRealExpiration()
            => ExpiresIn - (DateTimeOffset.UtcNow - AuthenticatedAt);

        public AuthorizationHeaders(TimeSpan? expiresIn = null)
        {
            ExpiresIn = expiresIn;
            AuthenticatedAt = DateTimeOffset.UtcNow;
        }

        private AuthorizationHeaders(OAuthHeaders oAuthHeaders)
        {
            AuthenticatedAt = DateTimeOffset.UtcNow;
            OAuthHeaders = oAuthHeaders;
            ExpiresIn = oAuthHeaders.ExpiresIn.HasValue ? TimeSpan.FromSeconds(oAuthHeaders.ExpiresIn.Value) : null;
            Add("Authorization", $"{oAuthHeaders.TokenType} {oAuthHeaders.AccessToken}");
        }

        public static implicit operator AuthorizationHeaders(OAuthHeaders headers)
        {
            ValidateValue(nameof(headers.AccessToken), headers.AccessToken);
            ValidateValue(nameof(headers.TokenType), headers.TokenType);
            if (headers.ExpiresIn is <= 0)
                throw new ArgumentException("ExpiresIn must be greater tha 0.");

            return new AuthorizationHeaders(headers);
        }
        
        private AuthorizationHeaders()
        {
        }
        
        private static void ValidateValue(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Property '{name}' is required.");
        }
    }
}
