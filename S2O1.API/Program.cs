using S2O1.DataAccess;
using Microsoft.EntityFrameworkCore;
using S2O1.Business;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using S2O1.Core.Interfaces;
using S2O1.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Data Access Layer
// Read connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) || 
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    var dockerConn = builder.Configuration.GetConnectionString("DockerConnection");
    if (!string.IsNullOrEmpty(dockerConn))
    {
        connectionString = dockerConn;
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    // Fallback or throw
    connectionString = "Server=localhost;Database=2S1O;User Id=sa;Password=Q1w2e3r4!;Encrypt=True;TrustServerCertificate=True;";
}
builder.Services.AddDataAccess(connectionString);

// 2. Business Layer
builder.Services.AddBusinessLayer();

// 3. API Specific Services
builder.Services.AddControllers();

// 4. Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// 5. Current User Service (Placeholder for API - typically reads from Claims)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, ApiCurrentUserService>();
builder.Services.AddHostedService<S2O1.API.Services.SystemRestartService>();
builder.Services.AddHostedService<S2O1.API.Services.QueueProcessorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
bool isDevelopment = app.Environment.IsDevelopment();

// Check database for DeploymentEnvironment setting if we can connect
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<S2O1.DataAccess.Contexts.S2O1DbContext>();
        if (context.Database.CanConnect())
        {
            var envSetting = context.SystemSettings.FirstOrDefault(s => s.SettingKey == "DeploymentEnvironment");
            if (envSetting != null && envSetting.SettingValue == "Development")
            {
                isDevelopment = true;
            }
        }
    }
    catch { } // Ignore DB errors at this stage
}

if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Ensure uploads directory exists
var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "waybills");
if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

app.UseStaticFiles(); // Serve files from wwwroot (uploads)
var currentDir = Directory.GetCurrentDirectory();
// Try different possible locations for 'web' folder
var possiblePaths = new[] 
{ 
    Path.Combine(currentDir, "web"),
    Path.Combine(currentDir, "..", "web"),
    Path.Combine(currentDir, "..", "..", "web")
};

string webPath = null;
foreach (var path in possiblePaths)
{
    if (Directory.Exists(path))
    {
        webPath = Path.GetFullPath(path);
        break;
    }
}

if (webPath != null)
{
    Console.WriteLine($"[INFO] Serving static files from: {webPath}");
    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webPath),
        RequestPath = "",
        EnableDirectoryBrowsing = false
    });
}
else 
{
    Console.WriteLine($"[WARNING] 'web' folder not found in: {string.Join(", ", possiblePaths)}");
}

app.UseAuthorization();

app.MapControllers();

// Ensure Database is Created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<S2O1.DataAccess.Contexts.S2O1DbContext>();
        
        // Bu kod şemayı otomatik günceller, verilere zarar vermez (eğer sadece yeni sütun/tablo eklendiyse)
        context.Database.Migrate();
        
        await S2O1.DataAccess.Persistence.DbInitializer.InitializeAsync(context, services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

app.Run();

