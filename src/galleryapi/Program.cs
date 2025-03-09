using GalleryAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;

var builder = WebApplication.CreateBuilder(args);
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Config the log
builder.Host.UseSerilog((context, config) =>
{
    var environment = context.HostingEnvironment;
    var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

    //config.MinimumLevel.Is(environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
    //     .Enrich.FromLogContext()
    //     .WriteTo.File(
    //         path: "../Logs/ACIDServer/log-.txt",
    //         rollingInterval: RollingInterval.Day, // 按天滚动
    //         outputTemplate: outputTemplate,
    //         retainedFileCountLimit: 14 // 保留最近7天日志
    //     );
    if (environment.IsDevelopment())
    {
        config.MinimumLevel.Is(LogEventLevel.Information)
             .Enrich.FromLogContext()
             .WriteTo.Console(theme: SystemConsoleTheme.Colored);

    }
    else if (environment.IsProduction())
    {
        config.MinimumLevel.Is(LogEventLevel.Warning)
             .Enrich.FromLogContext()
             .WriteTo.File(
                 path: "../Logs/galleryapi/log-.txt",
                 rollingInterval: RollingInterval.Day, // 按天滚动
                 outputTemplate: outputTemplate,
                 retainedFileCountLimit: 14 // 保留最近7天日志
             );
    }
});

// Connection string
var connstring = "";
if (builder.Environment.IsDevelopment())
    connstring = builder.Configuration["GalleryAPI:ConnectionString"];
else if (builder.Environment.IsProduction())
    connstring = builder.Configuration.GetConnectionString("AliyunConnection");

if (!String.IsNullOrEmpty(connstring))
    builder.Services.AddDbContext<GalleryContext>(opt => opt.UseSqlServer(connstring));

builder.Services.AddHttpContextAccessor();

IEdmModel model = EdmModelBuilder.GetEdmModel();
builder.Services.AddControllers().AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(100)
    .AddRouteComponents(model)
    .AddRouteComponents("v1", model)
    );

builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication("Bearer")
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

    builder.Services.AddCors(options =>
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
    builder.Services.AddAuthorization();
}
else if (builder.Environment.IsProduction())
{
    builder.Services.AddAuthentication("Bearer")
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
    builder.Services.AddCors(options =>
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
    builder.Services.AddAuthorization();
}

// Response Caching
builder.Services.AddResponseCaching();
// Memory cache
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseDollarOData();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseSerilogRequestLogging(); // <-- Add this line

// app.UseODataOpenApi();

// Add the OData Batch middleware to support OData $Batch
app.UseODataBatching();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData 8.x OpenAPI");
});

app.UseRouting().UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseResponseCaching();


















































