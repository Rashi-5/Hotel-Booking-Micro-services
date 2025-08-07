# Microservice Interconnection Documentation

## Overview
This document describes the interconnection between the Booking Service and Room Service in the hotel booking microservice architecture.

## Architecture

### Services
1. **Booking Service** (Port: 5211 - HTTP)
   - Handles booking creation, updates, and management
   - Communicates with Room Service for room availability and information
   - Stores booking data locally

2. **Room Service** (Port: 5238 - HTTP)
   - Manages room information and availability
   - Provides room details and availability checks
   - Stores room data locally

## Interconnection Flow

### 1. Room Information Retrieval
When the Booking Service needs room information:
- **Endpoint**: `GET /api/booking/rooms`
- **Flow**: Booking Service → Room Service → Returns room list
- **Implementation**: `BookingManager.GetAllRoomsAsync()` calls `IRoomServiceClient.GetAllRoomsAsync()`

### 2. Room Availability Check
When checking room availability:
- **Endpoint**: `GET /api/booking/availability`
- **Flow**: Booking Service → Room Service → Returns availability status
- **Implementation**: `BookingManager.CheckAvailabilityAsync()` calls `IRoomServiceClient.CheckRoomAvailabilityAsync()`

### 3. Booking Creation with Room Validation
When creating a booking:
- **Endpoint**: `POST /api/booking`
- **Flow**: 
  1. Booking Service → Room Service (get room details)
  2. Booking Service → Room Service (check availability)
  3. Booking Service → Local Repository (check existing bookings)
  4. Booking Service → Local Repository (save booking)
- **Implementation**: `BookingManager.CreateBookingAsync()` validates with Room Service first

## Configuration

### Booking Service Configuration (`BookingService/appsettings.json`)
```json
{
  "RoomService": {
    "BaseUrl": "http://localhost:5238"
  }
}
```

### Room Service Configuration (`RoomService/appsettings.json`)
```json
{
  "RoomService": {
    "BaseUrl": "http://localhost:5238"
  }
}
```

## Key Components

### Room Service Client (`BookingService/Repositories/RoomServiceClient.cs`)
- HTTP client for communicating with Room Service
- Methods:
  - `GetAllRoomsAsync()` - Get all available rooms
  - `GetRoomByNameAsync()` - Get specific room details
  - `CheckRoomAvailabilityAsync()` - Check room availability

### Room Service Availability Endpoint (`RoomService/Controllers/RoomController.cs`)
- **Endpoint**: `GET /api/room/availability`
- **Parameters**: `roomName`, `checkIn`, `checkOut`, `numberOfRooms`
- **Returns**: Availability status with detailed information

### Updated Booking Manager (`BookingService/Services/BookingManager.cs`)
- Now uses Room Service client for room operations
- Validates room availability before creating bookings
- Gets room information from Room Service instead of local data

## Testing

Use the provided `test-interconnection.http` file to test the interconnection:

1. Start both services:
   ```bash
   # Terminal 1
   cd BookingService && dotnet run
   
   # Terminal 2
   cd RoomService && dotnet run
   ```

2. Run the test requests in the HTTP file to verify:
   - Room Service endpoints work correctly
   - Booking Service calls Room Service for room information
   - Availability checks work across services
   - Booking creation validates with Room Service

## Error Handling

- **Room Service Unavailable**: Booking Service will return appropriate error messages
- **Room Not Found**: Clear error messages when room types don't exist
- **Availability Check Failures**: Detailed error messages for availability issues

## Future Enhancements

1. **Real-time Availability**: Implement real-time booking tracking
2. **Circuit Breaker**: Add circuit breaker pattern for service resilience
3. **Caching**: Add caching for frequently accessed room data
4. **Event-driven**: Implement event-driven architecture for booking updates
