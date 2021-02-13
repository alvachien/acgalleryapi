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

namespace GalleryAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public String ConnectionString { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // TBD
            // services.AddAuthentication();
            this.ConnectionString = Configuration["GalleryAPI:ConnectionString"];
            if (!String.IsNullOrEmpty(this.ConnectionString))
                services.AddDbContext<GalleryContext>(opt => opt.UseSqlServer(this.ConnectionString));

            services.AddControllers();

            IEdmModel model = EdmModelBuilder.GetEdmModel();

            services.AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(50)
                .AddModel(model)
                .AddModel("v1", model)
                // .AddModel("v2{data}", model2, builder => builder.AddService<ODataBatchHandler, DefaultODataBatchHandler>(Microsoft.OData.ServiceLifetime.Singleton))
                // .ConfigureRoute(route => route.EnableQualifiedOperationCall = false) // use this to configure the built route template
                );

            services.AddSwaggerGen();

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
            }

            app.UseCors(builder =>
#if DEBUG
                builder.WithOrigins(
                    "http://localhost:16001",
                    "https://localhost:16001"
                    )
#elif RELEASE
#if USE_AZURE
                builder.WithOrigins(
                    "https://acgallery.azurewebsites.net"
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

            app.UseHttpsRedirection();

            // TBD
            // app.UseAuthorization();

            // app.UseODataOpenApi();

            // Add the OData Batch middleware to support OData $Batch
            app.UseODataBatching();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData 8.x OpenAPI");
            });

            app.UseRouting();

            // a test middleware
            app.Use(next => context =>
            {
                var endpoint = context.GetEndpoint();
                if (endpoint == null)
                {
                    return next(context);
                }

                IEnumerable templates;
                IODataRoutingMetadata metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
                if (metadata != null)
                {
                    templates = metadata.Template.GetTemplates();
                }

                return next(context); // put a breaking point here
            });

            app.UseEndpoints(endpoints =>
            {
                if (env.IsDevelopment())
                {
                    // A odata debuger route is only for debugger view of the all OData endpoint routing.
                    endpoints.MapGet("/$odata", ODataRouteHandler.HandleOData);
                }

                endpoints.MapControllers();
            });

            app.UseResponseCaching();
        }
    }

    public class EntityReferenceODataDeserializerProvider : DefaultODataDeserializerProvider
    {
        public EntityReferenceODataDeserializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
        }

        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            return base.GetEdmTypeDeserializer(edmType);
        }

        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequest request)
        {
            return base.GetODataDeserializer(type, request);
        }
    }
}
