#!/bin/bash

echo "🧪 Testing Booking Form Field Name Fixes..."

# Wait for services to be ready
sleep 5

echo ""
echo "🔍 Testing Frontend Booking Page Load..."
curl -X GET "http://localhost:5125/Home/Booking" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s | grep -o "roomName.*" | head -3

echo ""
echo ""

echo "📅 Testing Booking Form Submission (simulating form data)..."
curl -X POST "http://localhost:5125/Home/SubmitBooking" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "CheckIn=2025-01-25&CheckOut=2025-01-27&Adult=2&Children=0&NumberOfRooms=1&RoomType=Standard%20Room&BookingType=Single&Frequency=&Interval=&Note=Test%20booking%20form%20fix" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo ""

echo "🔍 Testing Backend Booking Creation (direct API call)..."
curl -X POST "http://localhost:5211/api/Booking" \
  -H "Content-Type: application/json" \
  -d '{
    "Username": "testuser",
    "CustomerName": "Test User",
    "CheckIn": "2025-01-28T00:00:00",
    "CheckOut": "2025-01-30T00:00:00",
    "RoomType": "Standard Room",
    "NumberOfRooms": 1,
    "Note": "Test backend API",
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
echo "✅ Booking form field name fix tests completed!"
