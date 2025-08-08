using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingSystem.Models.Room
{
    public class RoomCardViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string RoomName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public List<string> Amenities { get; set; }
        public bool isDefault { get; set; }
        public string Price { get; set; }
        public int NumberOfRooms { get; set; }
    }
}