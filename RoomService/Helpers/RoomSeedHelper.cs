using HotelBookingSystem.Data;
using HotelBookingSystem.Helpers;
using Microsoft.EntityFrameworkCore;

public static class RoomSeedHelper
{
    public static async Task SeedDefaultRoomsAsync(RoomDbContext context, string xmlFilePath = "Data/default-rooms.xml")
    {
        Console.WriteLine("RoomSeedHelper: Starting XML-based seeding process...");
        
        try
        {
            // Read default rooms from XML file
            var defaultRooms = await XmlRoomReader.ReadDefaultRoomsFromXmlAsync(xmlFilePath);
            
            if (!defaultRooms.Any())
            {
                Console.WriteLine("RoomSeedHelper: No rooms found in XML. Using hardcoded fallback.");
                defaultRooms = DefaultRoomSeeder.DefaultRooms;
            }

            Console.WriteLine($"RoomSeedHelper: Adding {defaultRooms.Count} rooms to database...");

            // Reset IDs to let database auto-generate them
            foreach (var room in defaultRooms)
            {
                room.Id = 0;
                Console.WriteLine($"RoomSeedHelper: Adding room: {room.RoomName} - ${room.Price}");
            }
            
            context.Rooms.AddRange(defaultRooms);
            var result = await context.SaveChangesAsync();
            
            Console.WriteLine($"RoomSeedHelper: Saved {result} changes to database.");
            Console.WriteLine("RoomSeedHelper: XML-based seeding completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RoomSeedHelper: Error during XML seeding: {ex.Message}");
            Console.WriteLine("RoomSeedHelper: Falling back to hardcoded defaults...");
            
            // Fallback to hardcoded defaults
            var fallbackRooms = DefaultRoomSeeder.DefaultRooms;
            foreach (var room in fallbackRooms)
            {
                room.Id = 0; // Reset ID
            }
            
            context.Rooms.AddRange(fallbackRooms);
            var result = await context.SaveChangesAsync();
            
            Console.WriteLine($"RoomSeedHelper: Saved {result} fallback rooms to database.");
            Console.WriteLine("RoomSeedHelper: Fallback seeding completed!");
        }
    }
}
