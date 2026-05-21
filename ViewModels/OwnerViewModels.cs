using System.ComponentModel.DataAnnotations;
using Online_Booking_System.Models.Bookings;
using Microsoft.AspNetCore.Http;


namespace Online_Booking_System.ViewModels
{
    // ─── Dashboard ───────────────────────────────────────────────────────────────

    public class OwnerDashboardViewModel
    {
        public int TotalProperties { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        // Number of bookings the owner approved (customer can pay)
        public int ApprovedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<OwnerPropertySummaryViewModel> RecentProperties { get; set; } = [];
        public List<OwnerBookingSummaryViewModel> RecentBookings { get; set; } = [];
    }

    // ─── Property Summary (used in dashboard & list) ─────────────────────────────

    public class OwnerPropertySummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public string? ImageUrl { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int MaxGuests { get; set; }
        public int TotalBookings { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─── Booking Summary (used in dashboard & property bookings list) ─────────────

    public class OwnerBookingSummaryViewModel
    {
        public int BookingId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int GuestsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // ─── Create / Edit Property ───────────────────────────────────────────────────


    public class CreatePropertyViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Comma-separated list of facilities, e.g. "WiFi, Pool, Parking"</summary>
        public string? Facilities { get; set; }

        [Required]
        [Range(1, 100000)]
        public decimal PricePerNight { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [Url]
        public string? ImageUrl { get; set; }

        // File upload for main image (optional)
        public IFormFile? ImageFile { get; set; }

        /// <summary>Comma-separated gallery image URLs</summary>
        public string? GalleryImages { get; set; }

        public string? GoogleMapUrl { get; set; }

        [Required]
        [Range(1, 20)]
        public int Bedrooms { get; set; } = 1;

        [Required]
        [Range(1, 20)]
        public int Bathrooms { get; set; } = 1;

        [Required]
        [Range(1, 50)]
        public int MaxGuests { get; set; } = 2;
    }

    public class EditPropertyViewModel : CreatePropertyViewModel
    {
        public int Id { get; set; }
    }

    // ─── Property Bookings (owner views bookings for one of their properties) ─────

    public class PropertyBookingsViewModel
    {
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public List<OwnerBookingSummaryViewModel> Bookings { get; set; } = [];
    }
}
