#!/bin/bash

echo "🧪 Testing Room and Booking Fixes..."

# Wait for services to be ready
sleep 5

echo ""
echo "🏠 Testing Room Creation..."
curl -X POST "http://localhost:5238/api/Room" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "RoomName=Test%20Room&ImageUrl=/images/test.jpg&Description=Test%20room&Price=199.99&NumberOfRooms=3&isDefault=false&amenities=Wi-Fi&amenities=TV" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo ""

echo "📅 Testing Booking Creation..."
curl -X POST "http://localhost:5211/api/Booking" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "Username=testuser&CustomerName=Test%20User&CheckIn=2025-01-20&CheckOut=2025-01-22&RoomType=Standard%20Room&NumberOfRooms=1&Note=Test%20booking&Adult=2&Children=0&BookingType=Single&Frequency=&Interval=&TotalPrice=159.99" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "✅ Fix tests completed!"
