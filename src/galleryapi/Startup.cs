//#define USE_AZURE

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData;
using Microsoft.OData.Edm;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using GalleryAPI.Extensions;
using Microsoft.AspNetCore.OData.Batch;
using System.Collections;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using System.IO;
using Microsoft.IdentityModel.Tokens;

namespace GalleryAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;

            UploadFolder = Path.Combine(env.ContentRootPath, @"uploads");
            if (!Directory.Exists(UploadFolder))
            {
                Directory.CreateDirectory(UploadFolder);
            }
        }

        internal static String UploadFolder { get; private set; }
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public String ConnectionString { get; private set; }
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // TBD
            // services.AddAuthentication();
#if DEBUG
#if USE_AZURE
            this.ConnectionString = Configuration["GalleryAPI_Azure:ConnectionString"];
#else
            if (Environment.IsDevelopment())
                this.ConnectionString = Configuration["GalleryAPI:ConnectionString"];
#endif
#else
            if (Environment.IsProduction())           
                this.ConnectionString = Configuration.GetConnectionString("AliyunConnection");
#endif

            if (!String.IsNullOrEmpty(this.ConnectionString))
                services.AddDbContext<GalleryContext>(opt => opt.UseSqlServer(this.ConnectionString));

            services.AddHttpContextAccessor();

            IEdmModel model = EdmModelBuilder.GetEdmModel();
            services.AddControllers().AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(100)
                .AddRouteComponents(model)
                .AddRouteComponents("v1", model)
                );

            services.AddSwaggerGen();

            if (Environment.IsDevelopment())
            {
                services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.Authority = "https://localhost:44353";
                        options.RequireHttpsMetadata = true;
                        options.SaveToken = true;
                        options.IncludeErrorDetails = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false
                        };

                        options.Audience = "api.acgallery";
                    });

                services.AddCors(options =>
                {
                    options.AddPolicy(MyAllowSpecificOrigins, builder =>
                    {
                        builder.WithOrigins(
                            "https://localhost:16001"   // AC Gallery
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
                });
                services.AddAuthorization();
            }
            else if (Environment.IsProduction())
            {
                services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.Authority = "https://www.alvachien.com/idserver";
                        options.RequireHttpsMetadata = true;
                        options.SaveToken = true;
                        options.IncludeErrorDetails = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false
                        };

                        options.Audience = "api.acgallery";
                    });
                services.AddCors(options =>
                {
                    options.AddPolicy(MyAllowSpecificOrigins, builder =>
                    {
                        builder.WithOrigins(
                            "https://www.alvachien.com/gallery"   // AC Gallery
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
                });
                services.AddAuthorization();
            }

            // Response Caching
            services.AddResponseCaching();
            // Memory cache
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // app.UseDollarOData();
            }

            app.UseCors(MyAllowSpecificOrigins);

            app.UseHttpsRedirection();

            // app.UseODataOpenApi();

            // Add the OData Batch middleware to support OData $Batch
            app.UseODataBatching();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData 8.x OpenAPI");
            });

            app.UseRouting();

            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseResponseCaching();
        }
    }
}
