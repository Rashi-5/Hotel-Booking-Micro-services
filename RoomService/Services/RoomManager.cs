using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models.Room;
using HotelBookingSystem.Data;
using HotelBookingSystem.Repositories;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HotelBookingSystem.Services
{
    public class RoomManager
    {
        private readonly IRoomRepository _repository;
        private readonly IConfiguration _configuration;

        public RoomManager(IRoomRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync()
        {
            return await _repository.GetAllRoomsAsync();
        }

        public async Task<RoomCardViewModel> GetRoomByIdAsync(int id)
        {
            return await _repository.GetRoomByIdAsync(id);
        }

        public async Task<RoomCardViewModel> GetRoomByNameAsync(string roomName)
        {
            return await _repository.GetRoomByNameAsync(roomName);
        }

        public async Task<RoomCardViewModel> AddRoomAsync(RoomCardViewModel room)
        {
            // Validate room name uniqueness
            if (await _repository.RoomNameExistsAsync(room.RoomName))
            {
                throw new InvalidOperationException($"Room with name '{room.RoomName}' already exists.");
            }

            return await _repository.AddRoomAsync(room);
        }

        public async Task<RoomCardViewModel> UpdateRoomAsync(RoomCardViewModel room)
        {
            // Check if room exists
            if (!await _repository.RoomExistsAsync(room.Id))
            {
                throw new InvalidOperationException($"Room with ID {room.Id} not found.");
            }

            // Check if new name conflicts with other rooms
            var existingRoom = await _repository.GetRoomByIdAsync(room.Id);
            if (existingRoom.RoomName != room.RoomName && await _repository.RoomNameExistsAsync(room.RoomName))
            {
                throw new InvalidOperationException($"Room with name '{room.RoomName}' already exists.");
            }

            return await _repository.UpdateRoomAsync(room);
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var room = await _repository.GetRoomByIdAsync(id);
            if (room == null)
            {
                return false;
            }

            if (room.isDefault)
            {
                throw new InvalidOperationException("Cannot delete default room types.");
            }

            return await _repository.DeleteRoomAsync(id);
        }

        public async Task<bool> RoomExistsAsync(int id)
        {
            return await _repository.RoomExistsAsync(id);
        }

        public async Task<bool> RoomNameExistsAsync(string roomName)
        {
            return await _repository.RoomNameExistsAsync(roomName);
        }

        public string GetStorageType()
        {
            return _configuration["Storage:Type"] ?? "Database";
        }
    }
}
