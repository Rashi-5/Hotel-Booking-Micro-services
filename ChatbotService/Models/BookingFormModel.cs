namespace ChatbotService.Models
{
    public class BookingFormModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public int NumberOfRooms { get; set; }
        public decimal TotalPrice { get; set; }
        public string Note { get; set; } = string.Empty;
        public int Adult { get; set; }
        public int Children { get; set; }
        public string BookingType { get; set; } = string.Empty;
        public string? Frequency { get; set; }
        public int? Interval { get; set; }
        public List<string> Days { get; set; } = new();
    }
}
