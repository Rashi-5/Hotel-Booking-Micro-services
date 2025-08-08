using HotelBookingSystem.Models.Room;
using HotelBookingSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

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
                // Check if room name already exists within the same transaction
                var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomName == room.RoomName);
                if (existingRoom != null)
                {
                    throw new InvalidOperationException($"Room with name '{room.RoomName}' already exists.");
                }

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
                if (existingRoom == null)
                {
                    throw new InvalidOperationException($"Room with ID {room.Id} not found.");
                }

                // Check if new name conflicts with other rooms (excluding current room)
                var conflictingRoom = await _context.Rooms.FirstOrDefaultAsync(r => 
                    r.Id != room.Id && r.RoomName == room.RoomName);
                if (conflictingRoom != null)
                {
                    throw new InvalidOperationException($"Room with name '{room.RoomName}' already exists.");
                }

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