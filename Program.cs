using VesselMovementAPI.Data;
using Microsoft.EntityFrameworkCore;

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

// Database configuration - using PostgreSQL (cloud-ready)

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new Exception("No valid database connection string found.");



// Handle Heroku-style DATABASE_URL format
if (connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SslMode=Require;TrustServerCertificate=true";
}

Console.WriteLine(">>> CONNECTION STRING:\n" + connectionString);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline
    app.UseSwagger();
    app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database on startup
try
{
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    Console.WriteLine(">>> CONNECTION STRING:");
    Console.WriteLine(connectionString);
    dbContext.Database.EnsureCreated();
}
}
catch (Exception ex)
{
    Console.WriteLine("❌ DB Init Error: " + ex);
}
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();