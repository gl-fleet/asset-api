using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NuGet.Configuration;
using asset_allocation_api.Config;
using asset_allocation_api.Context;
using asset_allocation_api.Service.Background;
using asset_allocation_api.Service.Implementation;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ot_api_asset_allocation.Service.Middleware;
using Z.EntityFramework.Plus;

namespace ot_api_asset_allocation;

public class Startup
{
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        string[] allowedCors = ["localhost", "68.183.231.97"];

        app.UseCors(
            options => options.SetIsOriginAllowed(origin => allowedCors.Contains(new Uri(origin).Host)).AllowAnyMethod().AllowAnyHeader().AllowCredentials()
        );

        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
            AssetAllocationConfig.NameSpace + " " + AssetAllocationConfig.Version));
        // app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseMiddleware<RoleCheckerMiddleware>();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<SignalRHub>("/SignalR");
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheck.WriteListResponse
            });
        });
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure JWT authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //ValidIssuer = "",
                    //ValidAudience = "",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AssetAllocationConfig.JWT_SECRET)),
                };
            });
        services.AddSignalR(options =>
                {
                    options.EnableDetailedErrors = true;
                })
            .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                });

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(AssetAllocationConfig.assetAllocationConnectionString);
        });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",
                new OpenApiInfo { Title = AssetAllocationConfig.NameSpace, Version = AssetAllocationConfig.Version });
            c.TagActionsBy(api =>
            {
                if (api.GroupName != null) return [api.GroupName];

                if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    return [controllerActionDescriptor.ControllerName];

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });
            c.DocInclusionPredicate((_, _) => true);
        });
        services.AddHttpContextAccessor();


        string mode = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        services.AddHttpClient();
        services.AddScoped<DashboardService>();
        services.AddScoped<AssetAllocationHandler>();
        services.AddSingleton<SignalRHub>();
        services.AddHostedService<Scheduler>();
        // services.AddHostedService<AllocationConsumer>();
        services.AddHealthChecks().AddCheck<HealthCheck>(AssetAllocationConfig.NameSpace);
    }
}