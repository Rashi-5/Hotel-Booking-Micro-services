# Hotel Booking Microservices Integration Setup

## Overview
This guide will help you run the Hotel Booking system with the frontend integrated with the microservices.

## Prerequisites
- .NET 8.0 SDK
- SQLite (included with .NET)

## Service Ports
- **Frontend**: https://localhost:7111 (HTTPS) / http://localhost:5125 (HTTP)
- **AuthService**: http://localhost:5181 (HTTP)
- **BookingService**: http://localhost:5211 (HTTP)
- **RoomService**: http://localhost:5238 (HTTP)

## Quick Start

### 1. Start All Services
Open multiple terminal windows and run the following commands:

**Terminal 1 - AuthService:**
```bash
cd AuthService
dotnet run --urls="http://localhost:5181"
```

**Terminal 2 - BookingService:**
```bash
cd BookingService
dotnet run --urls="http://localhost:5211"
```

**Terminal 3 - RoomService:**
```bash
cd RoomService
dotnet run --urls="http://localhost:5238"
```

**Terminal 4 - Frontend:**
```bash
cd Hotel-Booking
dotnet run --urls="https://localhost:7111;http://localhost:5125"
```

### 2. Access the Application
- **Frontend**: https://localhost:7111
- **AuthService Swagger**: http://localhost:5181/swagger
- **BookingService Swagger**: http://localhost:5211/swagger
- **RoomService Swagger**: http://localhost:5238/swagger

## Configuration Files

### Frontend Configuration (Hotel-Booking/appsettings.json)
```json
{
  "Services": {
    "AuthService": {
      "BaseUrl": "http://localhost:5181"
    },
    "BookingService": {
      "BaseUrl": "http://localhost:5211"
    },
    "RoomService": {
      "BaseUrl": "http://localhost:5238"
    }
  }
}
```

## Features Integrated

### Authentication
- User registration and login through AuthService
- JWT token-based authentication
- Session management for frontend

### Room Management
- View available rooms from RoomService
- Room availability checking
- Room details and pricing

### Booking Management
- Create new bookings through BookingService
- View user's bookings
- Edit and delete bookings
- Check room availability

## Troubleshooting

### Common Issues

1. **CORS Errors**
   - Ensure all microservices are running
   - Check that CORS is properly configured in each service
   - Verify the frontend URLs in CORS configuration

2. **Connection Refused**
   - Check if all services are running on the correct ports
   - Verify the URLs in appsettings.json match the running services

3. **Authentication Issues**
   - Ensure AuthService is running and accessible
   - Check JWT token configuration
   - Verify the secret key is consistent across services

4. **Database Issues**
   - SQLite databases are created automatically
   - Check file permissions in the service directories

### Debug Steps

1. **Check Service Health**
   - Visit each service's Swagger UI to verify they're running
   - Test individual endpoints

2. **Check Frontend Logs**
   - Look for HTTP request errors in the browser console
   - Check the frontend application logs

3. **Verify Configuration**
   - Ensure all URLs in appsettings.json are correct
   - Check that ports are not being used by other applications

## Development Notes

### Architecture
- **Frontend**: ASP.NET Core MVC application
- **AuthService**: Handles user authentication and JWT tokens
- **BookingService**: Manages booking operations
- **RoomService**: Manages room information and availability

### Communication
- Frontend uses HTTP clients to communicate with microservices
- JWT tokens are automatically attached to requests
- CORS is configured to allow frontend communication

### Data Flow
1. User registers/logs in through AuthService
2. JWT token is stored in session
3. Frontend uses token for authenticated requests
4. Booking and room operations go through respective services

## Next Steps
- Add more robust error handling
- Implement service discovery
- Add monitoring and logging
- Consider using a message queue for async operations
