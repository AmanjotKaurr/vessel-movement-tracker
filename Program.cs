using VesselMovementAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database configuration with resilience
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new Exception("No valid database connection string found.");

// Handle Heroku-style DATABASE_URL format
if (connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SslMode=Require;TrustServerCertificate=true";
}

// Add connection pooling and resilience parameters
var enhancedConnectionString = connectionString + ";Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;";

Console.WriteLine(">>> CONNECTION STRING:\n" + enhancedConnectionString);

// Configure DbContext with resilience
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(enhancedConnectionString, npgsqlOptions =>
    {
        // Enable retry on failure for main transient error codes only
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: new[] { "57P01", "57P02", "57P03", "53300", "08006", "08001" }
        );
        // Set command timeout
        npgsqlOptions.CommandTimeout(30);
    });
    
    // Configure logging for debugging
    options.EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine, LogLevel.Information);
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database on startup with retry logic
await InitializeDatabaseAsync(app, enhancedConnectionString);

app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();

// Database initialization with retry logic
static async Task InitializeDatabaseAsync(WebApplication app, string connectionString)
{
    const int maxRetries = 5;
    const int retryDelayMs = 2000;
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"🔄 Database initialization attempt {attempt}/{maxRetries}");
            
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Test connection first
            await dbContext.Database.CanConnectAsync();
            Console.WriteLine("✅ Database connection test successful");
            
            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Database initialization completed successfully");
            
            return; // Success, exit retry loop
        }
        catch (NpgsqlException npgEx)
        {
            Console.WriteLine($"❌ NpgsqlException on attempt {attempt}: {npgEx.Message}");
            if (attempt == maxRetries)
            {
                Console.WriteLine("❌ Failed to initialize database after all retry attempts");
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception on attempt {attempt}: {ex.Message}");
            if (attempt == maxRetries)
            {
                Console.WriteLine("❌ Failed to initialize database after all retry attempts");
                throw;
            }
        }
        
        // Wait before retry
        await Task.Delay(retryDelayMs * attempt); // Exponential backoff
    }
}