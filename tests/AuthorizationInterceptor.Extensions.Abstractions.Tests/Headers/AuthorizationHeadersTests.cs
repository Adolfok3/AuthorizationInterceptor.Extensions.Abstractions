using AuthorizationInterceptor.Extensions.Abstractions.Headers;

namespace AuthorizationInterceptor.Extensions.Abstractions.Tests.Headers;

public class AuthorizationHeadersTests
{
    [Fact]
    public void CreateAuthorizationHeaders_WithDefaultProperties_ShouldContainsCorrectInformation()
    {
        //Arrange
        var headers = new AuthorizationHeaders();

        //Assert
        Assert.Null(headers.ExpiresIn);
        Assert.Null(headers.OAuthHeaders);
        Assert.Empty(headers);
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
    }

    [Fact]
    public void CreateAuthorizationHeaders_WithProperties_ShouldContainsCorrectInformation()
    {
        //Arrange
        var headers = new AuthorizationHeaders(TimeSpan.FromMinutes(3))
        {
            { "Test", "Test" }
        };

        //Assert
        Assert.NotNull(headers.ExpiresIn);
        Assert.NotEmpty(headers);
        Assert.NotEqual(headers.ExpiresIn, headers.GetRealExpiration());
        Assert.Contains(headers, a => a.Key == "Test" && a.Value == "Test");
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
    }

    [Fact]
    public void CreateAuthorizationHeaders_FromOAuth_WithCorrectProperties_ShouldCreateCorrectly()
    {
        //Arrange
        AuthorizationHeaders headers = new OAuthHeaders("Token", "Bearer", 3000, "refreskToken", "client_credentials");

        //Assert
        Assert.NotNull(headers.ExpiresIn);
        Assert.Equal(TimeSpan.FromSeconds(3000), headers.ExpiresIn);
        Assert.NotEmpty(headers);
        Assert.Contains(headers, a => a.Key == "Authorization" && a.Value == "Bearer Token");
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
        Assert.NotNull(headers.OAuthHeaders);
        Assert.Equal("Token", headers.OAuthHeaders.AccessToken);
        Assert.Equal("Bearer", headers.OAuthHeaders.TokenType);
        Assert.Equal(3000, headers.OAuthHeaders.ExpiresIn);
        Assert.Equal("refreskToken", headers.OAuthHeaders.RefreshToken);
        Assert.Equal("client_credentials", headers.OAuthHeaders.Scope);
    }

    [Fact]
    public void CreateAuthorizationHeaders_FromOAuth_WithCustomProperties_ShouldCreateCorrectly()
    {
        //Arrange
        AuthorizationHeaders headers = new OAuthHeaders("Token", "Bearer");

        //Assert
        Assert.Null(headers.ExpiresIn);
        Assert.NotEmpty(headers);
        Assert.Contains(headers, a => a.Key == "Authorization" && a.Value == "Bearer Token");
        Assert.NotEqual(DateTimeOffset.MinValue, headers.AuthenticatedAt);
    }

    [Fact]
    public void CreateAuthorizationHeaders_FromOAuth_WithWrongProperties_ShouldNotCreate()
    {
        //Arrange
        var entryWithToken = new OAuthHeaders("test", "");
        var entryWithType = new OAuthHeaders("", "test");
        var entryExpires = new OAuthHeaders("test", "test", -30000);

        //Act
        var act1 = () => (AuthorizationHeaders)entryWithToken;
        var act2 = () => (AuthorizationHeaders)entryWithType;
        var act3 = () => (AuthorizationHeaders)entryExpires;

        //Assert
        Assert.Equal("Property 'TokenType' is required.", Assert.Throws<ArgumentException>(act1).Message);
        Assert.Equal("Property 'AccessToken' is required.", Assert.Throws<ArgumentException>(act2).Message);
        Assert.Equal("ExpiresIn must be greater tha 0.", Assert.Throws<ArgumentException>(act3).Message);
    }
}
