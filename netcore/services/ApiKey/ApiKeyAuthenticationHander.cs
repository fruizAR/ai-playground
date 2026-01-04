using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyService apiKeyService)
        : base(options, logger, encoder)
    {
        _apiKeyService = apiKeyService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("x-api-key", out var apiKeyHeaderValues))
            return AuthenticateResult.Fail("x-api-key header is not found");

        var apiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
            return AuthenticateResult.Fail("x-api-key is missing");

        if (await _apiKeyService.ValidateApiKey(apiKey))
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "ApiKey") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Invalid API key");
    }
}