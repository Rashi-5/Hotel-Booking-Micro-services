using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Services;
using HotelBookingSystem.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

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

// Decide storage type from configuration
var storageType = builder.Configuration["Storage:Type"] ?? "Database";

if (storageType.Equals("XML", StringComparison.OrdinalIgnoreCase))
{
    var xmlFilePath = builder.Configuration["Storage:XmlFilePath"] ?? "rooms.xml";
    builder.Services.AddScoped<IRoomRepository>(provider => new XmlRoomRepository(xmlFilePath));
    
    XmlRoomSeeder.SeedDefaultRoomsIfNeeded(xmlFilePath);
}
else
{
    builder.Services.AddDbContext<RoomDbContext>(options =>
        options.UseSqlite("Data Source=room.db"));
    builder.Services.AddScoped<IRoomRepository, DatabaseRoomRepository>();
}

// Register services
builder.Services.AddScoped<RoomManager>();

// Add MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Room Service API", 
        Version = "v1" 
    });
    
    // Handle conflicting actions
    c.CustomSchemaIds(type => type.Name);
});

var app = builder.Build();

// Auto-seed default rooms only if using DB and table is empty
if (storageType.Equals("Database", StringComparison.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
        
        // Ensure database is created with latest schema
        context.Database.EnsureDeleted(); // Remove old database
        context.Database.EnsureCreated(); // Create new database with current schema
        
        // Seed default rooms
        await RoomSeedHelper.SeedDefaultRoomsAsync(context);
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors("AllowFrontend");

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
