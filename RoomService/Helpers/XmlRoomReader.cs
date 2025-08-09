using HotelBookingSystem.Models.Room;
using System.Xml.Serialization;

namespace HotelBookingSystem.Helpers
{
    public static class XmlRoomReader
    {
        public static async Task<List<RoomCardViewModel>> ReadDefaultRoomsFromXmlAsync(string xmlFilePath = "Data/default-rooms.xml")
        {
            try
            {
                if (!File.Exists(xmlFilePath))
                {
                    Console.WriteLine($"XML file not found: {xmlFilePath}");
                    return new List<RoomCardViewModel>();
                }

                var serializer = new XmlSerializer(typeof(List<RoomCardViewModel>));
                
                using var stream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read);
                var rooms = (List<RoomCardViewModel>?)serializer.Deserialize(stream);
                
                Console.WriteLine($"Successfully read {rooms?.Count ?? 0} rooms from XML file: {xmlFilePath}");
                return rooms ?? new List<RoomCardViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading rooms from XML file {xmlFilePath}: {ex.Message}");
                return new List<RoomCardViewModel>();
            }
        }

        public static async Task<bool> ValidateXmlRoomsAsync(string xmlFilePath = "Data/default-rooms.xml")
        {
            try
            {
                var rooms = await ReadDefaultRoomsFromXmlAsync(xmlFilePath);
                
                if (!rooms.Any())
                {
                    Console.WriteLine("No rooms found in XML file");
                    return false;
                }

                // Validate each room has required fields
                foreach (var room in rooms)
                {
                    if (string.IsNullOrWhiteSpace(room.RoomName) || 
                        string.IsNullOrWhiteSpace(room.Price) ||
                        room.NumberOfRooms <= 0)
                    {
                        Console.WriteLine($"Invalid room data found: {room.RoomName}");
                        return false;
                    }
                }

                Console.WriteLine($"XML validation successful: {rooms.Count} valid rooms found");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XML validation failed: {ex.Message}");
                return false;
            }
        }
    }
}
