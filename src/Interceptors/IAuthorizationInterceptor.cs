using AuthorizationInterceptor.Extensions.Abstractions.Headers;
using System.Threading.Tasks;

namespace AuthorizationInterceptor.Extensions.Abstractions.Interceptors
{
    /// <summary>
    /// Defines the interface for interceptor components responsible for managing authorization headers.
    /// </summary>
    public interface IAuthorizationInterceptor
    {
        /// <summary>
        /// Retrieves the current set of authorization headers.
        /// </summary>
        /// <returns>
        /// <see cref="AuthorizationHeaders"/> containing the authorization headers.
        /// </returns>
        Task<AuthorizationHeaders?> GetHeadersAsync();

        /// <summary>
        /// Update the current set of authorization headers.
        /// </summary>
        /// <param name="expiredHeaders">The old expired headers</param>
        /// <param name="newHeaders">The new valid headers</param>
        /// <returns></returns>
        Task UpdateHeadersAsync(AuthorizationHeaders? expiredHeaders, AuthorizationHeaders? newHeaders);
    }
}
