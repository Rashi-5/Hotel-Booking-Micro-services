using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models.Room
{
    public class RoomCardViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string RoomName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        // Store amenities as comma-separated string for EF Core compatibility
        public List<string> Amenities { get; set; }
        public bool isDefault { get; set; }
        public string Price { get; set; }
        public int NumberOfRooms { get; set; }
    }
}