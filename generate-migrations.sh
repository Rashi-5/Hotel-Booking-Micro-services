#!/bin/bash

echo "Generating EF Core Migrations for Azure SQL deployment..."

# Generate AuthService migrations
echo "🔧 AuthService Migrations..."
cd AuthService
dotnet ef migrations add InitialCreate --output-dir Migrations
cd ..

# Generate BookingService migrations  
echo "🔧 BookingService Migrations..."
cd BookingService
dotnet ef migrations add InitialCreate --output-dir Migrations
cd ..

# Generate RoomService migrations
echo "🔧 RoomService Migrations..."
cd RoomService
dotnet ef migrations add InitialCreate --output-dir Migrations
cd ..

# Generate ChatbotService migrations (if needed)
if [ -d "ChatbotService" ]; then
    echo "🔧 ChatbotService Migrations..."
    cd ChatbotService
    dotnet ef migrations add InitialCreate --output-dir Migrations
    cd ..
fi

echo "✅ All migrations generated successfully!"
echo ""
echo "📝 IMPORTANT: Before deploying to Azure:"
echo "1. Update appsettings.json with your Azure SQL connection strings"
echo "2. Set environment variables in Azure App Service"
echo "3. Test with Azure SQL Database"
