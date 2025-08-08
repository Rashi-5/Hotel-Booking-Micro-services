#!/bin/bash

echo "🧹 Cleaning and rebuilding all services..."

# Stop any running services
echo "🛑 Stopping existing services..."
pkill -f "dotnet.*AuthService" || true
pkill -f "dotnet.*BookingService" || true
pkill -f "dotnet.*RoomService" || true
pkill -f "dotnet.*Hotel-Booking" || true

sleep 2

# Clean and rebuild AuthService
echo "🔐 Cleaning and rebuilding AuthService..."
cd AuthService
dotnet clean
dotnet build
cd ..

# Clean and rebuild BookingService
echo "📅 Cleaning and rebuilding BookingService..."
cd BookingService
dotnet clean
dotnet build
cd ..

# Clean and rebuild RoomService
echo "🏠 Cleaning and rebuilding RoomService..."
cd RoomService
dotnet clean
dotnet build
cd ..

# Clean and rebuild Frontend
echo "🌐 Cleaning and rebuilding Frontend..."
cd Hotel-Booking
dotnet clean
dotnet build
cd ..

echo "✅ All services cleaned and rebuilt successfully!"
echo ""
echo "🚀 You can now run: ./start-services.sh"
