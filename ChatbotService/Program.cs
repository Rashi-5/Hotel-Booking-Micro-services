using Microsoft.EntityFrameworkCore;
using ChatbotService.Data;
using ChatbotService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7111", "http://localhost:5125", "https://localhost:5125")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Database - Azure SQL or SQLite
var connectionString = builder.Configuration.GetConnectionString("ChatbotDatabase") 
                     ?? builder.Configuration.GetConnectionString("DefaultConnection") 
                     ?? "Data Source=chatbot.db";
var isAzureSQL = connectionString.Contains("database.windows.net");

if (isAzureSQL)
{
    builder.Services.AddDbContext<ChatbotDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<ChatbotDbContext>(options =>
        options.UseSqlite(connectionString));
}

// Add HttpClient services for external APIs
builder.Services.AddHttpClient<IBookingServiceClient, BookingServiceClient>(client =>
{
    var baseUrl = builder.Configuration["Services:BookingService:BaseUrl"] ?? "http://localhost:5211";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IRoomServiceClient, RoomServiceClient>(client =>
{
    var baseUrl = builder.Configuration["Services:RoomService:BaseUrl"] ?? "http://localhost:5238";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add application services
builder.Services.AddScoped<IChatbotService, BotLogicService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Chatbot Service API", 
        Version = "v1",
        Description = "Hotel Booking Chatbot and Prediction Service"
    });
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ChatbotDbContext>();
    var dbConnectionString = builder.Configuration.GetConnectionString("ChatbotDatabase") 
                           ?? builder.Configuration.GetConnectionString("DefaultConnection") 
                           ?? "Data Source=chatbot.db";
    
    // Use Migrate for Azure SQL, EnsureCreated for SQLite
    if (dbConnectionString.Contains("database.windows.net"))
    {
        Console.WriteLine("Applying database migrations for Azure SQL...");
        await context.Database.MigrateAsync();
        Console.WriteLine("Azure SQL migrations completed");
    }
    else
    {
        var created = context.Database.EnsureCreated();
        Console.WriteLine($"SQLite database created: {created}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chatbot Service API v1");
    });
}

// Use CORS
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { 
    status = "healthy", 
    service = "ChatbotService",
    timestamp = DateTime.UtcNow 
});

app.Run();