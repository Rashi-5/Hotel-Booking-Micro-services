#!/bin/bash

echo "🔧 Fixing database issues..."

# Stop any running services
echo "🛑 Stopping existing services..."
pkill -f "dotnet.*AuthService" || true
pkill -f "dotnet.*BookingService" || true
pkill -f "dotnet.*RoomService" || true
pkill -f "dotnet.*Hotel-Booking" || true

sleep 2

# Remove old database files
echo "🗑️ Removing old database files..."
rm -f RoomService/room.db
rm -f RoomService/rooms.db
rm -f BookingService/booking.db
rm -f AuthService/auth.db

# Remove old migration files (except the first one)
echo "🗑️ Removing old migration files..."
cd RoomService
rm -f Migrations/20250807054312_InitialCreateDefaultRooms.cs
rm -f Migrations/20250807054312_InitialCreateDefaultRooms.Designer.cs
cd ..

cd BookingService
rm -f Migrations/*.cs
cd ..

# Clean and rebuild all services
echo "🧹 Cleaning and rebuilding services..."
cd RoomService
dotnet clean
dotnet build
cd ..

cd BookingService
dotnet clean
dotnet build
cd ..

cd AuthService
dotnet clean
dotnet build
cd ..

cd Hotel-Booking
dotnet clean
dotnet build
cd ..

echo "✅ Database fix completed!"
echo ""
echo "🚀 You can now run: ./start-services.sh"
echo "📝 The databases will be recreated with proper schemas when services start"
