namespace Online_Booking_System.ViewModels.Payment
{
    /// <summary>
    /// Returned after a payment session is created with the gateway.
    /// </summary>
    public class PaymentInitiationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        /// <summary>Internal transaction ID (our DB record).</summary>
        public int TransactionId { get; set; }

        /// <summary>Gateway-assigned session / intent ID.</summary>
        public string? GatewayTransactionId { get; set; }

        /// <summary>
        /// For hosted-checkout gateways (PayMob, PayPal): redirect the user here.
        /// Null for embedded flows (Stripe Elements).
        /// </summary>
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// For Stripe Elements: the client secret used to confirm the PaymentIntent.
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>Stripe publishable key, forwarded to the view.</summary>
        public string? PublishableKey { get; set; }

        public static PaymentInitiationResult Fail(string error) =>
            new() { Success = false, ErrorMessage = error };
    }
}
