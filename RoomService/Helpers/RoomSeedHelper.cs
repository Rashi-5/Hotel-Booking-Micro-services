using HotelBookingSystem.Data;
using Microsoft.EntityFrameworkCore;

public static class RoomSeedHelper
{
    public static async Task SeedDefaultRoomsAsync(RoomDbContext context)
    {
        if (!await context.Rooms.AnyAsync())
        {
            context.Rooms.AddRange(DefaultRoomSeeder.DefaultRooms);
            await context.SaveChangesAsync();
        }
    }
}
