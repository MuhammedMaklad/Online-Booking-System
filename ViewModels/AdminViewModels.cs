using System.ComponentModel.DataAnnotations;

namespace Online_Booking_System.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalProperties { get; set; }
        public int PendingProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalAdvertisements { get; set; }
        public int PendingAdvertisements { get; set; }
        public int CompletedPayments { get; set; }
        public decimal TotalPaymentRevenue { get; set; }
        public List<AdminRecentActivityViewModel> RecentActivity { get; set; } = [];
    }

    public class AdminRecentActivityViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminPropertyListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public string? ImageUrl { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminPropertyFilterViewModel
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public List<AdminPropertyListViewModel> Properties { get; set; } = [];
    }

    public class AdminReviewPropertyViewModel
    {
        [Required]
        public int PropertyId { get; set; }
        [Required]
        public string Action { get; set; } = string.Empty; 
        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    public class AdminAdvertisementListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminAdvertisementFilterViewModel
    {
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public List<AdminAdvertisementListViewModel> Advertisements { get; set; } = [];
    }

    public class AdminReviewAdvertisementViewModel
    {
        [Required]
        public int AdvertisementId { get; set; }
        [Required]
        public string Action { get; set; } = string.Empty;
        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    public class CreateAdvertisementViewModel
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Url]
        public string? ImageUrl { get; set; }

        // File upload for advertisement image
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }
    }
}