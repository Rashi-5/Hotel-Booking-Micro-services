using HotelBookingSystem.Data;
using Microsoft.EntityFrameworkCore;

public static class RoomSeedHelper
{
    public static async Task SeedDefaultRoomsAsync(RoomDbContext context)
    {
        Console.WriteLine("RoomSeedHelper: Starting seeding process...");
        
        // Always add default rooms (the calling method already checked if table is empty)
        Console.WriteLine($"RoomSeedHelper: Adding {DefaultRoomSeeder.DefaultRooms.Count} default rooms...");
        
        context.Rooms.AddRange(DefaultRoomSeeder.DefaultRooms);
        var result = await context.SaveChangesAsync();
        
        Console.WriteLine($"RoomSeedHelper: Saved {result} changes to database.");
        Console.WriteLine("RoomSeedHelper: Seeding completed successfully!");
    }
}
