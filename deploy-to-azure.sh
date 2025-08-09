#!/bin/bash

# Hotel Booking System - Azure Deployment Script
echo "🚀 Deploying Hotel Booking System to Azure..."

# Configuration - UPDATE THESE WITH YOUR AZURE VALUES
RESOURCE_GROUP="hotel-booking-rg"
SQL_SERVER="hotel-booking-sql-server"
SQL_ADMIN="sqladmin"
SQL_PASSWORD="YourSecurePassword123!"

# App Service Names - UPDATE THESE
AUTH_APP="hotel-auth-service"
BOOKING_APP="hotel-booking-service" 
ROOM_APP="hotel-room-service"
CHATBOT_APP="hotel-chatbot-service"
FRONTEND_APP="hotel-frontend"

echo "📝 Configuration:"
echo "Resource Group: $RESOURCE_GROUP"
echo "SQL Server: $SQL_SERVER"
echo "Apps: $AUTH_APP, $BOOKING_APP, $ROOM_APP, $CHATBOT_APP, $FRONTEND_APP"
echo ""

# Build all projects
echo "🔨 Building all projects..."
dotnet build AuthService/AuthService.csproj -c Release
dotnet build BookingService/BookingService.csproj -c Release
dotnet build RoomService/RoomService.csproj -c Release
dotnet build ChatbotService/ChatbotService.csproj -c Release
dotnet build Hotel-Booking/HotelBookingSystem.csproj -c Release

echo "✅ All projects built successfully!"

# Create deployment packages
echo "📦 Creating deployment packages..."
dotnet publish AuthService/AuthService.csproj -c Release -o ./publish/auth
dotnet publish BookingService/BookingService.csproj -c Release -o ./publish/booking
dotnet publish RoomService/RoomService.csproj -c Release -o ./publish/room
dotnet publish ChatbotService/ChatbotService.csproj -c Release -o ./publish/chatbot
dotnet publish Hotel-Booking/HotelBookingSystem.csproj -c Release -o ./publish/frontend

echo "✅ Deployment packages created!"

# Create zip files for deployment
echo "🗜️ Creating zip files..."
cd publish
zip -r auth-service.zip auth/
zip -r booking-service.zip booking/
zip -r room-service.zip room/
zip -r chatbot-service.zip chatbot/
zip -r frontend.zip frontend/
cd ..

echo "✅ Zip files created!"

echo ""
echo "🎯 NEXT STEPS:"
echo "1. Upload the following zip files to your Azure App Services:"
echo "   - publish/auth-service.zip → $AUTH_APP"
echo "   - publish/booking-service.zip → $BOOKING_APP"
echo "   - publish/room-service.zip → $ROOM_APP"
echo "   - publish/chatbot-service.zip → $CHATBOT_APP"
echo "   - publish/frontend.zip → $FRONTEND_APP"
echo ""
echo "2. Configure Application Settings in each App Service:"
echo "   - Connection strings for Azure SQL"
echo "   - JWT secret keys"
echo "   - Service URLs"
echo ""
echo "3. Enable CORS in each API service for your frontend domain"
echo ""
echo "4. Test the deployment!"

echo ""
echo "📚 Deployment files created in ./publish/ directory"
