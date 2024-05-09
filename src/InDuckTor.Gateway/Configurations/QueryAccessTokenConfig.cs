using InDuckTor.Shared.Security.Jwt;

namespace InDuckTor.Gateway.Configurations;

public static class QueryAccessTokenConfig
{
    private const string ProviderKey = "QueryToken";

    public static void ConfigureQueryAccessToken(this IServiceCollection serviceCollection,
        string configurationName,
        IConfigurationSection jwtConfig)
    {
        serviceCollection
            .AddAuthentication()
            .AddJwtBearer(ProviderKey, options =>
            {
                var jwtSettings = jwtConfig.Get<JwtSettings>() ??
                                  throw new ArgumentException(
                                      "Невозможно извлечь настройки JWT из конфигурации",
                                      configurationName);

                options.TokenValidationParameters = TokenValidationFactory.CreateTokenValidationParameters(jwtSettings);
                options.Events = new()
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
    }
}