using HotelBookingSystem.Models.Room;
using HotelBookingSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace HotelBookingSystem.Repositories
{
    public class DatabaseRoomRepository : IRoomRepository
    {
        private readonly RoomDbContext _context;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public DatabaseRoomRepository(RoomDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Rooms.ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<RoomCardViewModel> GetRoomByIdAsync(int id)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<RoomCardViewModel> GetRoomByNameAsync(string roomName)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Rooms.FirstOrDefaultAsync(r => r.RoomName == roomName);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<RoomCardViewModel> AddRoomAsync(RoomCardViewModel room)
        {
            await _semaphore.WaitAsync();
            try
            {
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                return room;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<RoomCardViewModel> UpdateRoomAsync(RoomCardViewModel room)
        {
            await _semaphore.WaitAsync();
            try
            {
                var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == room.Id);
                if (existingRoom != null)
                {
                    existingRoom.RoomName = room.RoomName;
                    existingRoom.ImageUrl = room.ImageUrl;
                    existingRoom.Description = room.Description;
                    existingRoom.Amenities = room.Amenities;
                    existingRoom.isDefault = room.isDefault;
                    existingRoom.Price = room.Price;
                    existingRoom.NumberOfRooms = room.NumberOfRooms;
                    await _context.SaveChangesAsync();
                    return existingRoom;
                }
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            await _semaphore.WaitAsync();
            try
            {
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
                if (room == null || room.isDefault)
                    return false;

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                return true;
                    }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> RoomExistsAsync(int id)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Rooms.AnyAsync(r => r.Id == id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> RoomNameExistsAsync(string roomName)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Rooms.AnyAsync(r => r.RoomName == roomName);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
} 