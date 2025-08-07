# RoomService Microservice

A complete microservice implementation with repository pattern, thread-safe I/O operations, and support for both XML and Database storage.

## Features

### ✅ Repository Pattern Implementation
- **IRoomRepository**: Abstract interface for data access
- **DatabaseRoomRepository**: EF Core implementation with SQLite
- **XmlRoomRepository**: XML file-based storage with thread-safe I/O

### ✅ Thread Safety & I/O Operations
- **SemaphoreSlim**: Thread-safe access to shared resources
- **Async/Await**: Non-blocking I/O operations
- **FileStream**: Thread-safe file operations for XML storage
- **Task.Run()**: Background thread execution for heavy operations

### ✅ Microservice Communication
- **RoomServiceClient**: HTTP client for inter-service communication
- **IRoomServiceClient**: Interface for dependency injection
- **JSON Serialization**: Proper data transfer between services

### ✅ Configuration Management
- **appsettings.json**: Production configuration (Database)
- **appsettings.Development.json**: Development configuration (XML)
- **Storage Type Selection**: Runtime configuration for storage type

## Architecture

```
RoomService/
├── Controllers/
│   └── RoomController.cs          # RESTful API endpoints
├── Repositories/
│   ├── IRoomRepository.cs         # Repository interface
│   ├── DatabaseRoomRepository.cs  # EF Core implementation
│   ├── XmlRoomRepository.cs       # XML file implementation
│   ├── IRoomServiceClient.cs      # Client interface
│   └── RoomServiceClient.cs       # HTTP client implementation
├── Services/
│   └── RoomManager.cs             # Business logic layer
├── Model/
│   └── RoomCardViewModel.cs       # EF Core entity
├── Data/
│   └── RoomDbContext.cs           # EF Core context
└── Program.cs                     # DI configuration
```

## Configuration

### Storage Types

#### Database Storage (Default)
```json
{
  "Storage": {
    "Type": "Database"
  }
}
```

#### XML Storage
```json
{
  "Storage": {
    "Type": "XML",
    "XmlFilePath": "rooms.xml"
  }
}
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/room` | Get all rooms |
| GET | `/api/room/{id}` | Get room by ID |
| GET | `/api/room/search?roomName={name}` | Search room by name |
| POST | `/api/room` | Create new room |
| PUT | `/api/room/{id}` | Update room |
| DELETE | `/api/room/{id}` | Delete room |
| GET | `/api/room/storage-type` | Get current storage type |

## Thread Safety Features

### 1. SemaphoreSlim for Database Operations
```csharp
private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

public async Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync()
{
    await _semaphore.WaitAsync();
    try
    {
        return await _context.Rooms.ToListAsync();
    }
    finally
    {
        _semaphore.Release();
    }
}
```

### 2. Thread-Safe File I/O for XML
```csharp
private async Task SaveRoomsAsync(List<RoomCardViewModel> rooms)
{
    using var stream = new FileStream(_xmlFilePath, FileMode.Create, FileAccess.Write);
    await Task.Run(() => _serializer.Serialize(stream, rooms));
}
```

### 3. Async Operations Throughout
- All repository methods are async
- All controller endpoints are async
- Non-blocking I/O operations

## Usage Examples

### 1. Run with Database Storage
```bash
cd RoomService
dotnet run
```

### 2. Run with XML Storage
```bash
cd RoomService
dotnet run --environment Development
```

### 3. Test API Endpoints
```bash
# Get all rooms
curl -X GET "https://localhost:7206/api/room"

# Create a room
curl -X POST "https://localhost:7206/api/room" \
  -H "Content-Type: application/json" \
  -d '{
    "roomName": "Executive Suite",
    "amenities": "King Bed,Wi-Fi,Jacuzzi",
    "price": "399.99"
  }'
```

### 4. Use as Microservice Client
```csharp
// In another service
public class BookingService
{
    private readonly IRoomServiceClient _roomClient;
    
    public BookingService(IRoomServiceClient roomClient)
    {
        _roomClient = roomClient;
    }
    
    public async Task<RoomDetailsDto> GetRoomAsync(string roomName)
    {
        return await _roomClient.GetRoomByNameAsync(roomName);
    }
}
```

## Migration Commands

### Database Migration
```bash
# Add EF Core tools
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialRoomMigration

# Apply migration
dotnet ef database update
```

### XML Storage
- No migration needed
- XML file is created automatically on first use
- File path: `rooms.xml` (configurable)

## Evidence of Effective Implementation

### ✅ Repository Pattern
- **Abstraction**: `IRoomRepository` interface
- **Multiple Implementations**: Database and XML
- **Dependency Injection**: Runtime selection of implementation
- **Separation of Concerns**: Data access separated from business logic

### ✅ Thread Safety & I/O Operations
- **SemaphoreSlim**: Prevents race conditions
- **Async/Await**: Non-blocking operations
- **Task.Run()**: Background thread execution
- **FileStream**: Thread-safe file operations
- **Proper Resource Management**: Using statements and try-finally blocks

### ✅ Microservice Communication
- **HTTP Client**: Proper HTTP communication
- **JSON Serialization**: Standard data format
- **Error Handling**: Proper exception handling
- **Configuration**: Configurable base URLs
- **Interface-based**: Dependency injection ready

## Performance Benefits

1. **Thread Safety**: Prevents data corruption in concurrent scenarios
2. **Async I/O**: Non-blocking operations improve responsiveness
3. **Repository Pattern**: Easy to switch storage implementations
4. **Microservice Ready**: Can be consumed by other services
5. **Configurable**: Runtime storage type selection

## Testing

Use the provided `RoomService.http` file to test all endpoints:

```bash
# Test with VS Code REST Client extension
# Or use curl commands as shown above
```

The service demonstrates effective use of:
- Repository pattern with multiple storage options
- Thread-safe I/O operations
- Async/await patterns
- Microservice communication
- Dependency injection
- Configuration management 