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
            // ADDED: Validate room name doesn't already exist
            if (await _repository.RoomNameExistsAsync(room.RoomName))
            {
                throw new InvalidOperationException($"A room with the name '{room.RoomName}' already exists.");
            }

            // ADDED: Basic validation
            if (string.IsNullOrWhiteSpace(room.RoomName))
            {
                throw new InvalidOperationException("Room name is required.");
            }

            if (room.NumberOfRooms <= 0)
            {
                throw new InvalidOperationException("Number of rooms must be greater than 0.");
            }

            return await _repository.AddRoomAsync(room);
        }

        public async Task<RoomCardViewModel> UpdateRoomAsync(RoomCardViewModel room)
        {
            // ADDED: Check if room exists
            var existingRoom = await _repository.GetRoomByIdAsync(room.Id);
            if (existingRoom == null)
            {
                throw new InvalidOperationException($"Room with ID {room.Id} not found.");
            }

            // ADDED: Check if new name conflicts with another room (if name is being changed)
            if (existingRoom.RoomName != room.RoomName)
            {
                var roomWithSameName = await _repository.GetRoomByNameAsync(room.RoomName);
                if (roomWithSameName != null && roomWithSameName.Id != room.Id)
                {
                    throw new InvalidOperationException($"A room with the name '{room.RoomName}' already exists.");
                }
            }

            // ADDED: Basic validation
            if (string.IsNullOrWhiteSpace(room.RoomName))
            {
                throw new InvalidOperationException("Room name is required.");
            }

            if (room.NumberOfRooms <= 0)
            {
                throw new InvalidOperationException("Number of rooms must be greater than 0.");
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
