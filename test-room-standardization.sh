#!/bin/bash

echo "🧪 Testing Room Standardization..."

# Wait for services to be ready
sleep 5

echo ""
echo "🏠 Testing Room Creation with RoomCardViewModel..."
curl -X POST "http://localhost:5238/api/Room" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "RoomName=Standardized%20Room&ImageUrl=/images/test.jpg&Description=Test%20room%20with%20standardized%20model&Price=199.99&NumberOfRooms=3&isDefault=false&amenities=Wi-Fi&amenities=TV&amenities=King%20Bed" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo ""

echo "🏠 Testing Room Update with RoomCardViewModel..."
curl -X PUT "http://localhost:5238/api/Room/1" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "Id=1&RoomName=Updated%20Standardized%20Room&ImageUrl=/images/standard.jpg&Description=Updated%20description&Price=159.99&NumberOfRooms=5&isDefault=true&amenities=Wi-Fi&amenities=TV&amenities=Air%20Conditioning" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo ""

echo "🔍 Testing Room Retrieval..."
curl -X GET "http://localhost:5238/api/Room" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s | jq '.[0:2] | .[] | {id: .id, roomName: .roomName, amenities: .amenities}'

echo ""
echo "✅ Room standardization tests completed!"
