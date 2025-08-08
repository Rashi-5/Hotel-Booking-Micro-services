using HotelBookingSystem.Models.Room;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Serialization;
using System.Xml;

namespace HotelBookingSystem.Repositories
{
    public class XmlRoomRepository : IRoomRepository
    {
        private readonly string _xmlFilePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly XmlSerializer _serializer;

        public XmlRoomRepository(string xmlFilePath = "rooms.xml")
        {
            _xmlFilePath = xmlFilePath;
            _serializer = new XmlSerializer(typeof(List<RoomCardViewModel>));
        }

        public async Task<IEnumerable<RoomCardViewModel>> GetAllRoomsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_xmlFilePath))
                {
                    return new List<RoomCardViewModel>();
                }

                using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read);
                // Remove Task.Run wrapper - this was causing potential issues
                var rooms = (List<RoomCardViewModel>)_serializer.Deserialize(stream);
                return rooms ?? new List<RoomCardViewModel>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<RoomCardViewModel> GetRoomByIdAsync(int id)
        {
            var rooms = await GetAllRoomsAsync();
            return rooms.FirstOrDefault(r => r.Id == id);
        }

        public async Task<RoomCardViewModel> GetRoomByNameAsync(string roomName)
        {
            var rooms = await GetAllRoomsAsync();
            return rooms.FirstOrDefault(r => r.RoomName == roomName);
        }

        public async Task<RoomCardViewModel> AddRoomAsync(RoomCardViewModel room)
        {
            await _semaphore.WaitAsync();
            try
            {
                // FIXED: Don't call GetAllRoomsAsync() here as it will try to acquire the same semaphore
                var rooms = await GetAllRoomsInternalAsync();
                
                // Generate new ID if not set
                if (room.Id == 0)
                {
                    room.Id = rooms.Count > 0 ? rooms.Max(r => r.Id) + 1 : 1;
                }

                rooms.Add(room);
                await SaveRoomsInternalAsync(rooms);
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
                // FIXED: Don't call GetAllRoomsAsync() here as it will try to acquire the same semaphore
                var rooms = await GetAllRoomsInternalAsync();
                var existingRoom = rooms.FirstOrDefault(r => r.Id == room.Id);
                
                if (existingRoom != null)
                {
                    existingRoom.RoomName = room.RoomName;
                    existingRoom.ImageUrl = room.ImageUrl;
                    existingRoom.Description = room.Description;
                    existingRoom.Amenities = room.Amenities;
                    existingRoom.isDefault = room.isDefault;
                    existingRoom.Price = room.Price;
                    existingRoom.NumberOfRooms = room.NumberOfRooms;
                    
                    await SaveRoomsInternalAsync(rooms);
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
                // FIXED: Don't call GetAllRoomsAsync() here as it will try to acquire the same semaphore
                var rooms = await GetAllRoomsInternalAsync();
                var room = rooms.FirstOrDefault(r => r.Id == id);
                
                if (room != null)
                {
                    rooms.Remove(room);
                    await SaveRoomsInternalAsync(rooms);
                    return true;
                }
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> RoomExistsAsync(int id)
        {
            var rooms = await GetAllRoomsAsync();
            return rooms.Any(r => r.Id == id);
        }

        public async Task<bool> RoomNameExistsAsync(string roomName)
        {
            var rooms = await GetAllRoomsAsync();
            return rooms.Any(r => r.RoomName == roomName);
        }

        // ADDED: Internal method that doesn't use semaphore (assumes caller already has it)
        private async Task<List<RoomCardViewModel>> GetAllRoomsInternalAsync()
        {
            if (!File.Exists(_xmlFilePath))
            {
                return new List<RoomCardViewModel>();
            }

            using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read);
            var rooms = (List<RoomCardViewModel>)_serializer.Deserialize(stream);
            return rooms ?? new List<RoomCardViewModel>();
        }

        // ADDED: Internal save method that doesn't use Task.Run
        private async Task SaveRoomsInternalAsync(List<RoomCardViewModel> rooms)
        {
            var directory = Path.GetDirectoryName(_xmlFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var stream = new FileStream(_xmlFilePath, FileMode.Create, FileAccess.Write);
            _serializer.Serialize(stream, rooms);
            await Task.CompletedTask; // Make it async-compatible
        }

        // Keep the original for backward compatibility
        private async Task SaveRoomsAsync(List<RoomCardViewModel> rooms)
        {
            await SaveRoomsInternalAsync(rooms);
        }
    }
}