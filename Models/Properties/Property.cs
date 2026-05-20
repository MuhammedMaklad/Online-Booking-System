using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string? Facilities { get; set; }
        public double Rating { get; set; }

        public int ReviewsCount { get; set; }
        [MaxLength]
        public string? GoogleMapUrl { get; set; }

        public string? GalleryImages { get; set; }

        [Required]
        [Range(1, 100000)]
        public decimal PricePerNight { get; set; }

        [Required]
        [MaxLength]
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

        // Owner relationship
        public string? OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public ApplicationUser? Owner { get; set; }

        public PropertyStatus Status { get; set; } = PropertyStatus.Pending;
        public string? AdminNotes { get; set; }
    }
}