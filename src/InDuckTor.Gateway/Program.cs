using InDuckTor.Shared.Security.Jwt;
using Microsoft.AspNetCore;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var webHostBuilder = WebHost.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        var configsDir = Path.Combine("OcelotConfigs", context.HostingEnvironment.EnvironmentName);
        var ocelotConfigs = Directory.GetFiles(configsDir, "ocelot.*.json");
        foreach (var ocelotConfig in ocelotConfigs)
        {
            builder.AddJsonFile(ocelotConfig, true, true);
        }
    })
    .ConfigureServices((context, services) =>
    {
        services.AddEndpointsApiExplorer();
        services.AddInDuckTorAuthentication(context.Configuration.GetSection(nameof(JwtSettings)));
        services.AddOcelot();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Api Gateway", Version = "v1" });
        });
        services.AddSwaggerForOcelot(context.Configuration, options =>
        {
            options.GenerateDocsForGatewayItSelf = true;
        });
    })
    .Configure((context, applicationBuilder) =>
    {
        if (!context.HostingEnvironment.IsProduction())
        {
            applicationBuilder.UseSwaggerForOcelotUI(options => { options.ServerOcelot = "/swagger"; });
        }

        applicationBuilder
            .UseAuthentication()
            .UseOcelot()
            .Wait();
    });


var app = webHostBuilder.Build();

app.Run();