#!/bin/bash

echo "🧪 Testing Final Booking Functionality..."

# Wait for services to be ready
sleep 5

echo ""
echo "📅 Testing Booking Creation..."
curl -X POST "http://localhost:5211/api/Booking" \
  -H "Content-Type: application/json" \
  -d '{
    "Username": "testuser",
    "CustomerName": "Test User",
    "CheckIn": "2025-01-25T00:00:00",
    "CheckOut": "2025-01-27T00:00:00",
    "RoomType": "Standard Room",
    "NumberOfRooms": 1,
    "Note": "Final test booking",
    "Adult": 2,
    "Children": 0,
    "BookingType": "Single",
    "Frequency": "",
    "Interval": null,
    "TotalPrice": 0,
    "Days": [],
    "Amenities": []
  }' \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo ""

echo "📅 Testing Booking Update..."
curl -X PUT "http://localhost:5211/api/Booking/00000000-0000-0000-0000-000000000000" \
  -H "Content-Type: application/json" \
  -d '{
    "BookingId": "00000000-0000-0000-0000-000000000000",
    "Username": "testuser",
    "CustomerName": "Updated User",
    "CheckIn": "2025-01-30T00:00:00",
    "CheckOut": "2025-02-01T00:00:00",
    "RoomType": "Standard Room",
    "NumberOfRooms": 1,
    "Note": "Updated booking",
    "Adult": 2,
    "Children": 0,
    "BookingType": "Single",
    "Frequency": "",
    "Interval": null,
    "TotalPrice": 0,
    "Days": [],
    "Amenities": []
  }' \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo ""

echo "🔍 Testing Room Availability..."
curl -X GET "http://localhost:5238/api/Room/availability?roomName=Standard%20Room&checkIn=2025-01-25&checkOut=2025-01-27&numberOfRooms=1" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "✅ Final booking tests completed!"
