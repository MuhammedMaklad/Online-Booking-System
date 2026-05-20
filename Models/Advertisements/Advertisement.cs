using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Online_Booking_System.Models;

namespace Online_Booking_System.Models.Advertisements
{
    public class Advertisement
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [ForeignKey(nameof(OwnerId))]
        public ApplicationUser? Owner { get; set; }

        public AdvertisementStatus Status { get; set; } = AdvertisementStatus.Pending;
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }
    }
}