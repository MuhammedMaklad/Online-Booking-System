using Online_Booking_System.Models.Payments;
using Online_Booking_System.ViewModels.Payment;

namespace Online_Booking_System.Contracts.Payment
{
    /// <summary>
    /// Strategy interface — every gateway (Stripe, PayMob, PayPal …) implements this.
    /// </summary>
    public interface IPaymentProvider
    {
        /// <summary>Identifies which <see cref="PaymentMethod"/> this provider handles.</summary>
        PaymentMethod Method { get; }

        /// <summary>
        /// Initiates a payment session.
        /// Returns a result that may contain a redirect URL (hosted checkout)
        /// or a client secret (embedded checkout like Stripe Elements).
        /// </summary>
        Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentRequest request);

        /// <summary>
        /// Verifies and captures a payment after the user returns from the gateway.
        /// </summary>
        Task<PaymentVerificationResult> VerifyPaymentAsync(string gatewayTransactionId, string? additionalData = null);

        /// <summary>
        /// Processes an inbound webhook payload from the gateway.
        /// Returns the updated transaction status.
        /// </summary>
        Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature);

        /// <summary>
        /// Issues a full or partial refund for a completed transaction.
        /// </summary>
        Task<RefundResult> RefundAsync(string gatewayTransactionId, decimal amount, string reason);
    }
}
