#if DEBUG
#else
#define USE_AZURE
//define USE_ALIYUN
#endif

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;

namespace acgalleryapi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;

            UploadFolder = env.ContentRootPath + @"\\uploads";
            if (!Directory.Exists(UploadFolder))
            {
                Directory.CreateDirectory(UploadFolder);
            }
        }

        public IConfiguration Configuration { get; }
        internal static String UploadFolder { get; private set; }
        public IHostingEnvironment HostingEnvironment { get; private set; }

        internal static String DBConnectionString { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Add framework services.
            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters()
                .AddAuthorization(
                    options => {
                        options.AddPolicy("GalleryAdmin", policy => policy.RequireRole("GalleryAdmin"));
                        options.AddPolicy("GalleryPro", policy => policy.RequireRole("GalleryPro"));
                        options.AddPolicy("FileSizeRequirementPolicy",
                            policy =>
                            {
                                policy.AuthenticationSchemes.Add("Bearer");
                                policy.RequireAuthenticatedUser();
                                policy.Requirements.Add(new FileUploadSizeRequirement());
                            });
                    }
                );

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
#if DEBUG
                    options.Authority = "http://localhost:41016";
#else
#if USE_AZURE
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

            services.AddSingleton<IAuthorizationHandler, FileUploadSizeHandler>();

#if DEBUG
            DBConnectionString = Configuration.GetConnectionString("DebugConnection");
#else
#if USE_AZURE
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
#if USE_AZURE
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

            //app.UseStaticFiles(); // For the wwwroot folder
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"uploads")),
                RequestPath = new PathString("/uploads"),
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                }
            });

            app.UseMvc();
        }
    }
}
