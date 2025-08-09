namespace ChatbotService.Models
{
    public class RoomCardViewModel
    {
        public int Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Amenities { get; set; } = new();
        public bool isDefault { get; set; }
        public string Price { get; set; } = string.Empty;
        public int NumberOfRooms { get; set; }
    }
}
