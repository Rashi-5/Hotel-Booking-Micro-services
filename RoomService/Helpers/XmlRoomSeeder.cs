using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using HotelBookingSystem.Models.Room;

public static class XmlRoomSeeder
{
    public static void SeedDefaultRoomsIfNeeded(string xmlFilePath)
    {
        if (!File.Exists(xmlFilePath) || new FileInfo(xmlFilePath).Length == 0)
        {
            var defaultRooms = DefaultRoomSeeder.DefaultRooms;
            
            var serializer = new XmlSerializer(typeof(List<RoomCardViewModel>));
            using var writer = new StreamWriter(xmlFilePath);
            serializer.Serialize(writer, defaultRooms);
        }
    }
}
