using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Services;
using HotelBookingSystem.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

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

// Register HttpClient for inter-service communication
builder.Services.AddHttpClient<IRoomServiceClient, RoomServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RoomService:BaseUrl"] ?? "https://localhost:5238");
});

// Add MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-seed default rooms only if using DB and table is empty
if (storageType.Equals("Database", StringComparison.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
        await RoomSeedHelper.SeedDefaultRoomsAsync(context);
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
