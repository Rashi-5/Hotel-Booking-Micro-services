# 🚀 Azure Deployment Guide - Hotel Booking Microservices

## 📋 Prerequisites
- [x] Azure App Service (created)
- [x] Azure SQL Database Server (created)
- [x] Azure SQL Database (created)
- [ ] Azure CLI or Visual Studio/VS Code with Azure extensions

## 🏗️ Architecture Overview
```
Frontend (App Service) → API Gateway → Microservices (App Services) → Azure SQL Database
```

## 🔧 Step 1: Prepare Your Azure Resources

### 1.1 App Services Needed
Create these App Services in Azure:
- `hotel-auth-service` (AuthService)
- `hotel-booking-service` (BookingService)
- `hotel-room-service` (RoomService)
- `hotel-chatbot-service` (ChatbotService)
- `hotel-frontend` (MVC Frontend)

### 1.2 Database Setup
Create these databases in your Azure SQL Server:
- `AuthDB` (Users and authentication)
- `BookingDB` (Bookings data)
- `RoomDB` (Room types and availability)
- `ChatbotDB` (Chat history, optional)

## 🔧 Step 2: Update Configuration Files

### 2.1 Get Your Connection Strings
1. Go to Azure Portal → SQL Database → Connection Strings
2. Copy the ADO.NET connection string
3. Replace `{your_username}` and `{your_password}` with your SQL admin credentials

Example:
```
Server=hotel-booking-sql-server.database.windows.net;Database=AuthDB;User Id=sqladmin;Password=YourPassword123!;TrustServerCertificate=true;
```

### 2.2 Update appsettings.Production.json Files
Replace `YOUR_*` placeholders in these files:
- `AuthService/appsettings.Production.json`
- `BookingService/appsettings.Production.json`
- `RoomService/appsettings.Production.json`
- `Hotel-Booking/appsettings.Production.json`

**Example for AuthService:**
```json
{
  "ConnectionStrings": {
    "AuthDatabase": "Server=hotel-booking-sql-server.database.windows.net;Database=AuthDB;User Id=sqladmin;Password=YourPassword123!;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "MyVerySecureJwtSecretKey32CharactersLong123456",
    "Issuer": "HotelBookingSystem",
    "Audience": "HotelBookingSystemUsers"
  },
  "CORS": {
    "AllowedOrigins": [
      "https://hotel-frontend.azurewebsites.net"
    ]
  }
}
```

### 2.3 Update Service URLs
In `Hotel-Booking/appsettings.Production.json`:
```json
{
  "ServiceUrls": {
    "AuthService": {
      "BaseUrl": "https://hotel-auth-service.azurewebsites.net"
    },
    "BookingService": {
      "BaseUrl": "https://hotel-booking-service.azurewebsites.net"
    },
    "RoomService": {
      "BaseUrl": "https://hotel-room-service.azurewebsites.net"
    },
    "ChatbotService": {
      "BaseUrl": "https://hotel-chatbot-service.azurewebsites.net"
    }
  }
}
```

## 🔧 Step 3: Generate Database Migrations

Run this command to generate EF Core migrations:
```bash
chmod +x generate-migrations.sh
./generate-migrations.sh
```

## 🔧 Step 4: Build and Deploy

### 4.1 Automated Deployment
```bash
chmod +x deploy-to-azure.sh
./deploy-to-azure.sh
```

### 4.2 Manual Deployment Steps
1. **Build Projects:**
   ```bash
   dotnet build AuthService/AuthService.csproj -c Release
   dotnet build BookingService/BookingService.csproj -c Release
   dotnet build RoomService/RoomService.csproj -c Release
   dotnet build ChatbotService/ChatbotService.csproj -c Release
   dotnet build Hotel-Booking/HotelBookingSystem.csproj -c Release
   ```

2. **Publish Projects:**
   ```bash
   dotnet publish AuthService/AuthService.csproj -c Release -o ./publish/auth
   dotnet publish BookingService/BookingService.csproj -c Release -o ./publish/booking
   dotnet publish RoomService/RoomService.csproj -c Release -o ./publish/room
   dotnet publish ChatbotService/ChatbotService.csproj -c Release -o ./publish/chatbot
   dotnet publish Hotel-Booking/HotelBookingSystem.csproj -c Release -o ./publish/frontend
   ```

3. **Create Zip Files:**
   ```bash
   cd publish
   zip -r auth-service.zip auth/
   zip -r booking-service.zip booking/
   zip -r room-service.zip room/
   zip -r chatbot-service.zip chatbot/
   zip -r frontend.zip frontend/
   ```

4. **Upload to Azure:**
   - Use Azure Portal → App Service → Deployment Center
   - Or use Azure CLI: `az webapp deployment source config-zip`

## 🔧 Step 5: Configure Azure App Services

### 5.1 Application Settings
For each App Service, add these settings in Azure Portal:

**AuthService Settings:**
```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__AuthDatabase = [Your Auth DB Connection String]
JwtSettings__SecretKey = [Your JWT Secret]
```

**BookingService Settings:**
```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__BookingDatabase = [Your Booking DB Connection String]
JwtSettings__SecretKey = [Your JWT Secret]
RoomService__BaseUrl = https://hotel-room-service.azurewebsites.net
```

**RoomService Settings:**
```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__RoomDatabase = [Your Room DB Connection String]
JwtSettings__SecretKey = [Your JWT Secret]
```

**Frontend Settings:**
```
ASPNETCORE_ENVIRONMENT = Production
ServiceUrls__AuthService__BaseUrl = https://hotel-auth-service.azurewebsites.net
ServiceUrls__BookingService__BaseUrl = https://hotel-booking-service.azurewebsites.net
ServiceUrls__RoomService__BaseUrl = https://hotel-room-service.azurewebsites.net
ServiceUrls__ChatbotService__BaseUrl = https://hotel-chatbot-service.azurewebsites.net
JwtSettings__SecretKey = [Your JWT Secret]
```

### 5.2 CORS Configuration
Each API service needs CORS configured for your frontend domain:

1. Go to Azure Portal → App Service → CORS
2. Add allowed origin: `https://hotel-frontend.azurewebsites.net`
3. Enable "Access-Control-Allow-Credentials"

## 🔧 Step 6: Database Setup

### 6.1 Run Initial Migrations
The applications will automatically run migrations on first startup thanks to our code changes.

### 6.2 Verify Database Creation
Check Azure Portal → SQL Database → Query Editor to verify tables are created:
- AuthDB: `Users` table
- BookingDB: `Bookings` table
- RoomDB: `Rooms` table

## 🔧 Step 7: Testing

### 7.1 Test Services Individually
1. **AuthService:** `https://hotel-auth-service.azurewebsites.net/swagger`
2. **BookingService:** `https://hotel-booking-service.azurewebsites.net/swagger`
3. **RoomService:** `https://hotel-room-service.azurewebsites.net/swagger`
4. **ChatbotService:** `https://hotel-chatbot-service.azurewebsites.net/swagger`

### 7.2 Test Frontend
Visit: `https://hotel-frontend.azurewebsites.net`

## 🚨 Troubleshooting

### Common Issues:

1. **Database Connection Errors:**
   - Check connection strings in App Settings
   - Verify SQL Server firewall allows Azure services
   - Ensure database names match configuration

2. **CORS Errors:**
   - Check CORS settings in each API service
   - Verify frontend URL is in allowed origins

3. **JWT Authentication Errors:**
   - Ensure JWT secret is the same across all services
   - Check JWT issuer/audience settings

4. **Service Communication Errors:**
   - Verify service URLs in frontend configuration
   - Check that all services are running

### Debugging Steps:
1. Check App Service logs in Azure Portal
2. Enable Application Insights for monitoring
3. Test API endpoints with Swagger UI
4. Verify database connectivity with Query Editor

## 🎯 Post-Deployment Checklist

- [ ] All 5 App Services deployed and running
- [ ] Database tables created and seeded
- [ ] CORS configured for all API services
- [ ] Frontend can connect to all API services
- [ ] User registration/login works
- [ ] Room booking functionality works
- [ ] Chatbot is responsive
- [ ] SSL certificates are properly configured

## 🔒 Security Considerations

1. **Use Azure Key Vault** for storing secrets (JWT keys, connection strings)
2. **Enable HTTPS** for all services
3. **Configure Azure AD** for additional authentication (optional)
4. **Enable Application Insights** for monitoring
5. **Set up Azure Monitor** for alerts

## 💰 Cost Optimization

1. Use **B1 Basic** tier for App Services (sufficient for testing)
2. Use **Basic** tier for SQL Database initially
3. Configure **auto-scaling** based on usage
4. Monitor costs with Azure Cost Management

---

🎉 **Congratulations!** Your Hotel Booking Microservices are now deployed to Azure!

For support, check Azure documentation or contact Azure support.
