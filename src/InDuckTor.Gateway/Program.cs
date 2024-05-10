using InDuckTor.Gateway.Configurations;
using InDuckTor.Shared.Security.Jwt;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var webHostBuilder = WebHost.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        var configsDir = Path.Combine("OcelotConfigs", context.HostingEnvironment.EnvironmentName);
        builder.AddOcelotWithSwaggerSupport(options =>
        {
            options.Folder = configsDir;
            options.FileOfSwaggerEndPoints = "ocelot.swagger";
        });
    })
    .ConfigureServices((context, services) =>
    {
        var jwtConfig = context.Configuration.GetSection(nameof(JwtSettings));
        services.ConfigureQueryAccessToken(nameof(context.Configuration), jwtConfig);
        services.AddEndpointsApiExplorer();
        services.AddInDuckTorAuthentication(jwtConfig)
            .AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("Bearer", QueryAccessTokenConfig.ProviderKey)
                .Build());
        services.AddOcelot();
        services.AddSwaggerGen(
            options => { options.SwaggerDoc("v1", new() { Title = "Api Gateway", Version = "v1" }); });
        services.AddSwaggerForOcelot(context.Configuration,
            options => { options.GenerateDocsForGatewayItSelf = true; });
    })
    .Configure((context, applicationBuilder) =>
    {
        if (!context.HostingEnvironment.IsProduction())
        {
            applicationBuilder.UseSwaggerForOcelotUI(options => { options.ServerOcelot = "/swagger"; });
        }

        applicationBuilder
            .UseAuthentication()
            .UseWebSockets()
            .UseOcelot()
            .Wait();
    });


var app = webHostBuilder.Build();

app.Run();