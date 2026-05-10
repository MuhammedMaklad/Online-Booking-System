using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Online_Booking_System.Models.Bookings;

namespace Online_Booking_System.Models.Payments
{
    /// <summary>
    /// Persists every payment attempt for audit and reconciliation.
    /// </summary>
    public class PaymentTransaction
    {
        public int Id { get; set; }

        // ── Booking link ──────────────────────────────────────────────────────
        [Required]
        public int BookingId { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;

        // ── User link ─────────────────────────────────────────────────────────
        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        // ── Payment details ───────────────────────────────────────────────────
        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // ── Gateway data ──────────────────────────────────────────────────────

        /// <summary>ID returned by the payment gateway (e.g. Stripe PaymentIntent ID).</summary>
        [StringLength(500)]
        public string? GatewayTransactionId { get; set; }

        /// <summary>Raw response payload stored for debugging / reconciliation.</summary>
        public string? GatewayResponse { get; set; }

        /// <summary>URL the user should be redirected to for hosted checkout pages.</summary>
        [StringLength(2000)]
        public string? RedirectUrl { get; set; }

        // ── Timestamps ────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }

        // ── Failure info ──────────────────────────────────────────────────────
        [StringLength(500)]
        public string? FailureReason { get; set; }
    }
}
