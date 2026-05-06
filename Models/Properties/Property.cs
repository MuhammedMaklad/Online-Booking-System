using System.ComponentModel.DataAnnotations;

namespace Online_Booking_System.Models.Properties
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 100000)]
        public decimal PricePerNight { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Range(1, 20)]
        public int Bedrooms { get; set; }

        [Range(1, 20)]
        public int Bathrooms { get; set; }

        [Range(1, 50)]
        public int MaxGuests { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}