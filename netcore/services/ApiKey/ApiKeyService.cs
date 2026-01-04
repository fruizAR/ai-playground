using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

public interface IApiKeyService
{
    Task<bool> ValidateApiKey(string apiKey);
}
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    // You can add any specific options for your API key authentication here
}
public class ApiKeyService : IApiKeyService
{
    private readonly AppDBContext db;

    public ApiKeyService(AppDBContext dbContext)
    {
        db = dbContext;
    }

    public async Task<bool> ValidateApiKey(string apiKey)
    {
        //TODO: In some implementations with no tenant configuration, consider to add a validation from a configuration file or environment variable
        var apiKeyGuid = Guid.Parse(apiKey);
        return await db.Tenants.AnyAsync(t => t.TenantId == apiKeyGuid);
    }
}