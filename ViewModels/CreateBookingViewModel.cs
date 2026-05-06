using System.ComponentModel.DataAnnotations;

namespace Online_Booking_System.ViewModels
{
    public class CreateBookingViewModel
    {
        public int PropertyId { get; set; }

        [Required]
        public DateTime CheckIn { get; set; }

        [Required]
        public DateTime CheckOut { get; set; }

        [Range(1, 20)]
        public int GuestsCount { get; set; } = 1;
    }
}