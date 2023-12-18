using System.Diagnostics.CodeAnalysis;

using FrontendRegulatorAccountEnrollment.Core.Configs;
using FrontendRegulatorAccountEnrollment.Core.Services;
using FrontendRegulatorAccountEnrollment.Web.Configs;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Identity.Web;
using StackExchange.Redis;

namespace FrontendRegulatorAccountEnrollment.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceProviderExtension
    {
        public static IServiceCollection RegisterCoreComponents(this IServiceCollection services, IConfiguration configuration)
        {
            bool useMockData = configuration.GetValue<bool>("FacadeAPI:UseMockData");

            if (useMockData)
            {
                services.AddSingleton<IFacadeService, MockedFacadeService>();
            }
            else
            {
                services.AddHttpClient<IFacadeService, FacadeService>(c => c.Timeout = TimeSpan
                    .FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")));
            }

            services.AddScoped<IAllowlistService, AllowlistService>();

            return services;
        }

        public static IServiceCollection RegisterWebComponents(
            this IServiceCollection services, IConfiguration configuration)
        {
            ConfigureOptions(services, configuration);
            ConfigureAuthentication(services, configuration);
            ConfigureAuthorization(services);
            ConfigureSession(services, configuration);

            return services;
        }

        private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FacadeApiConfig>(configuration.GetSection(FacadeApiConfig.ConfigSection));
            services.Configure<RegulatorsDetails>(configuration.GetSection(RegulatorsDetails.ConfigSection));
            services.Configure<PhaseBannerOptions>(configuration.GetSection(PhaseBannerOptions.ConfigSection));
            services.Configure<AppConfig>(configuration.GetSection(AppConfig.ConfigSection));
        }

        private static void ConfigureSession(IServiceCollection services, IConfiguration configuration)
        {
            bool useLocalSession = configuration.GetValue<bool>("UseLocalSession");

            if (!useLocalSession)
            {
                string? redisConnection = configuration.GetConnectionString("REDIS_CONNECTION");

                // EPR Regulators
                services.AddDataProtection()
                    .SetApplicationName("EprProducers")
                    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConnection), "DataProtection-Keys");

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = configuration.GetValue<string>("RedisInstanceName");
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services.AddSession(options =>
            {
                options.Cookie.Name = configuration.GetValue<string>("CookieOptions:SessionCookieName");
                options.IdleTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("SessionIdleTimeOutMinutes"));
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Path = configuration.GetValue<string>("PATH_BASE");
            });
        }

        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    configuration.GetSection("AzureAdB2C").Bind(options);
                    options.ErrorPath = "/Enrolment/Error";
                }, options =>
                {
                    options.Cookie.Name = configuration.GetValue<string>("CookieOptions:AuthenticationCookieName");
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue<int>("CookieOptions:AuthenticationExpiryInMinutes"));
                    options.SlidingExpiration = true;
                    options.Cookie.Path = "/";
                })
                .EnableTokenAcquisitionToCallDownstreamApi(new string[] { configuration.GetValue<string>("FacadeAPI:DownstreamScope") })
                .AddDistributedTokenCaches();
        }

        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }
    }
}