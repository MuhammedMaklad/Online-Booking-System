
using System.ComponentModel.DataAnnotations;

namespace Online_Booking_System.ViewModels
{
    public class BookingsViewModel
    {
        [Key]
        public int BookingId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string PropertyCity { get; set; } = string.Empty;
        public string? PropertyImageUrl { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int GuestsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
