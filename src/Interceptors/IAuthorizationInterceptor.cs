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
        /// <param name="name">Name of the integration or HttpClient</param>
        /// <returns>
        /// <see cref="AuthorizationHeaders"/> containing the authorization headers.
        /// </returns>
        Task<AuthorizationHeaders?> GetHeadersAsync(string name);

        /// <summary>
        /// Update the current set of authorization headers.
        /// </summary>
        /// <param name="name">Name of the integration or HttpClient</param>
        /// <param name="expiredHeaders">The old expired headers</param>
        /// <param name="newHeaders">The new valid headers</param>
        /// <returns></returns>
        Task UpdateHeadersAsync(string name, AuthorizationHeaders? expiredHeaders, AuthorizationHeaders? newHeaders);
    }
}
