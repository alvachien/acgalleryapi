using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using GalleryAPI.test.common;
using GalleryAPI.Models;
using Microsoft.Extensions.Hosting;
using Xunit;
using Microsoft.AspNetCore.TestHost;

namespace GalleryAPI.integrationtest
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected SqliteConnection DBConnection { get; private set; }

        public CustomWebApplicationFactory() {
            DBConnection = new SqliteConnection("DataSource=:memory:");
            DBConnection.Open();

            try
            {
                // Create the schema in the database
                var context = GetCurrentDataContext();
                if (!context.Database.IsSqlite()
                    || context.Database.IsSqlServer())
                {
                    throw new Exception("Faield!");
                }

                // Create tables and views
                DataSetupUtility.CreateDatabaseTables(context.Database);
                DataSetupUtility.CreateDatabaseViews(context.Database);

                context.Database.EnsureCreated();

                // Setup the tables

                context.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                // Error occurred
            }
            finally
            {
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (DBConnection != null)
            {
                DBConnection.Close();
                DBConnection = null;
            }
        }

        public GalleryContext GetCurrentDataContext()
        {
            var options = new DbContextOptionsBuilder<GalleryContext>()
                .UseSqlite(DBConnection, action =>
                {
                    action.UseRelationalNulls();
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new GalleryContext(options, true);
            return context;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureTestServices((services) =>
                        {
                            services
                                .AddControllers()
                                .AddApplicationPart(typeof(Startup).Assembly);
                        });
                });

            return builder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseStartup<TStartup>();
            base.ConfigureWebHost(builder);

            //builder.ConfigureServices(services =>
            //{
            //    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GalleryContext>));
            //    services.Remove(descriptor);

            //    services.AddDbContext<GalleryContext>(options =>
            //    {                    
            //        options.UseSqlite(DBConnection, action =>
            //        {
            //             action.UseRelationalNulls();
            //        })
            //        .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
            //        .EnableSensitiveDataLogging();
            //    });

            //    var sp = services.BuildServiceProvider();
            //    using (var scope = sp.CreateScope())
            //    {
            //        var scopedServices = scope.ServiceProvider;
            //        var db = scopedServices.GetRequiredService<GalleryContext>();
            //        //var logger = scopedServices
            //        //    .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

            //        db.Database.EnsureCreated();

            //        try
            //        {
            //            // Utilities.InitializeDbForTests(db);
            //        }
            //        catch (Exception exp)
            //        {

            //            //logger.LogError(ex, "An error occurred seeding the " +
            //            //    "database with test messages. Error: {Message}", ex.Message);
            //        }
            //    }
            //});
        }
    }
}
