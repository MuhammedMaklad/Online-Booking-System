using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Online_Booking_System.Models
{
  public class ApplicationUser:IdentityUser
  {
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(300)]
    public string? Bio { get; set; }

    // Account Lifecycle
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    //public ICollection<Listing> Listings { get; set; } = [];
    //public ICollection<Booking> Bookings { get; set; } = [];

    // Computed
    public string FullName => $"{FirstName} {LastName}";

  }
}
