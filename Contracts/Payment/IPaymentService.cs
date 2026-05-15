using Online_Booking_System.Models.Payments;
using Online_Booking_System.ViewModels.Payment;

namespace Online_Booking_System.Contracts.Payment
{
    /// <summary>
    /// Orchestrates payment operations across all providers.
    /// Controllers talk to this service, not to individual providers.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Starts a payment for a booking using the chosen method.
        /// Creates a <see cref="PaymentTransaction"/> record and returns initiation details.
        /// </summary>
        Task<PaymentInitiationResult> InitiatePaymentAsync(
            int bookingId,
            string userId,
            PaymentMethod method,
            string currency = "USD");

        /// <summary>
        /// Verifies a payment after the user returns from the gateway.
        /// Updates the <see cref="PaymentTransaction"/> and the <see cref="Booking"/> status.
        /// </summary>
        Task<PaymentVerificationResult> VerifyPaymentAsync(
            int transactionId,
            string gatewayTransactionId,
            string? additionalData = null);

        /// <summary>
        /// Handles an inbound webhook from any gateway.
        /// Identifies the provider by <paramref name="method"/> and delegates processing.
        /// </summary>
        Task<WebhookProcessingResult> HandleWebhookAsync(
            PaymentMethod method,
            string payload,
            string signature);

        /// <summary>
        /// Refunds a completed payment transaction.
        /// </summary>
        Task<RefundResult> RefundAsync(int transactionId, decimal amount, string reason);

        /// <summary>Returns all transactions for a booking.</summary>
        Task<IEnumerable<PaymentTransactionViewModel>> GetTransactionsByBookingAsync(int bookingId);

        /// <summary>Returns a single transaction by its internal ID.</summary>
        Task<PaymentTransactionViewModel?> GetTransactionByIdAsync(int transactionId);

        Task<IEnumerable<PaymentTransactionViewModel>> GetAllTransactionsAsync();

    }
}
