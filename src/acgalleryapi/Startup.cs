
#define USE_MICROSOFTAZURE
//define USE_ALIYUN

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace acgalleryapi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
#if DEBUG
            env.EnvironmentName = "Development";
#else
            env.EnvironmentName = "Production";
#endif

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        internal static String DBConnectionString { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Add framework services.
            services.AddMvcCore()
                .AddJsonFormatters()
                .AddAuthorization(
                    options => {
                        options.AddPolicy("GalleryAdmin", policy => policy.RequireRole("GalleryAdmin"));
                        options.AddPolicy("GalleryPro", policy => policy.RequireRole("GalleryPro"));
                    }
                );

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
#if DEBUG
                    options.Authority = "http://localhost:41016";
#else
#if USE_MICROSOFTAZURE
                    options.Authority = "http://acidserver.azurewebsites.net";
#elif USE_ALIYUN
                    options.Authority = "http://118.178.58.187:5100/";
#endif
#endif
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api.galleryapi";
                    //options.AutomaticAuthenticate = true;
                    //options.AutomaticChallenge = true;
                });

#if DEBUG
            DBConnectionString = Configuration.GetConnectionString("DebugConnection");
#else
#if USE_MICROSOFTAZURE
            DBConnectionString = Configuration.GetConnectionString("AzureConnection");
#elif USE_ALIYUN
            DBConnectionString = Configuration.GetConnectionString("AliyunConnection");
#endif
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();

            app.UseCors(builder =>
#if DEBUG
                builder.WithOrigins(
                    "http://localhost:1601",
                    "https://localhost:1601"
                    )
#else
#if USE_MICROSOFTAZURE
                builder.WithOrigins(
                    "http://acgallery.azurewebsites.net/",
                    "https://acgallery.azurewebsites.net/"
                    )
#elif USE_ALIYUN
                builder.WithOrigins(
                    "http://118.178.58.187:5210",
                    "https://118.178.58.187:5210"
                    )
#endif
#endif
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                );

            app.UseMvc();
        }
    }
}
