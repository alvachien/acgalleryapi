using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Routing.Conventions;
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

            services.AddOData();
            services.AddRouting();

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

            IEdmModel model = EdmModelBuilder.GetEdmModel();

            // Please add "UseODataBatching()" before "UseRouting()" to support OData $batch.
            app.UseODataBatching();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapODataRoute(
                    "nullPrefix", null,
                    b =>
                    {
                        b.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model);
                        b.AddService<ODataDeserializerProvider>(Microsoft.OData.ServiceLifetime.Singleton, sp => new EntityReferenceODataDeserializerProvider(sp));
                        b.AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton,
                            sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting("nullPrefix", endpoints.ServiceProvider));
                    });

                endpoints.MapODataRoute("odataPrefix", "odata", model);

                //endpoints.MapODataRoute("myPrefix", "my/{data}", model);

                //endpoints.MapODataRoute("msPrefix", "ms", model, new DefaultODataBatchHandler());
            });


            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});

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
