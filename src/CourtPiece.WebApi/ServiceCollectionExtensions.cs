using ManagedCode.Orleans.SignalR.Core.Config;
using ManagedCode.Orleans.SignalR.Server.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Orleans.Configuration;
using System.Text;

namespace CourtPiece.WebApi
{
    public static class ServiceCollectionExtensions
    {
        public static IHostBuilder ConfigureOrleans(this IHostBuilder host)
        {
            return host.UseOrleans(siloBuilder =>
            {

                 siloBuilder.UseLocalhostClustering();

                 //siloBuilder.AddMemoryStreams("test").AddMemoryGrainStorage("PubSubStore");

                 siloBuilder.AddFileGrainStorage("File");

                 siloBuilder.Configure<GrainCollectionOptions>(i =>
                 {
                     i.CollectionQuantum = TimeSpan.FromSeconds(10);
                     i.CollectionAge = TimeSpan.FromSeconds(20);
                     i.ClassSpecificCollectionAge.Add(nameof(RoomManager),TimeSpan.FromMinutes(5));

                 });

                 siloBuilder.ConfigureOrleansSignalR();
                 siloBuilder.AddMemoryGrainStorage(OrleansSignalROptions.OrleansSignalRStorage);
                 siloBuilder.Services
                 .AddSignalR()
                 .AddOrleans();
             });
        }

        public static IServiceCollection ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // For Identity  
            services.AddIdentity<AppUser, IdentityRole<int>>()
                            .AddEntityFrameworkStores<ApplicationDbContext>()
                            .AddDefaultTokenProviders();
            // Adding Authentication  
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Adding Jwt Bearer  
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["JWTKey:ValidAudience"],
                    ValidIssuer = configuration["JWTKey:ValidIssuer"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTKey:Secret"]))
                };
            });
            return services;

        }
    }
}
