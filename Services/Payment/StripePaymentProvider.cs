using Microsoft.Extensions.Options;
using Online_Booking_System.Contracts.Payment;
using Online_Booking_System.Settings;
using Online_Booking_System.ViewModels.Payment;
using Stripe;
using AppPaymentMethod = Online_Booking_System.Models.Payments.PaymentMethod;
using AppPaymentStatus = Online_Booking_System.Models.Payments.PaymentStatus;

namespace Online_Booking_System.Services.Payment
{
    /// <summary>
    /// Stripe provider using PaymentIntents API (embedded checkout via Stripe.js).
    /// </summary>
    public class StripePaymentProvider : IPaymentProvider
    {
        private readonly StripeSettings _settings;
        private readonly ILogger<StripePaymentProvider> _logger;

        public AppPaymentMethod Method => AppPaymentMethod.Stripe;

        public StripePaymentProvider(
            IOptions<StripeSettings> settings,
            ILogger<StripePaymentProvider> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }

        public async Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Stripe uses smallest currency unit
                    Currency = request.Currency.ToLower(),
                    Description = request.Description,
                    Metadata = new Dictionary<string, string>
                    {
                        ["booking_id"]     = request.BookingId.ToString(),
                        ["transaction_id"] = request.TransactionId.ToString(),
                        ["user_id"]        = request.UserId
                    },
                    ReceiptEmail = request.UserEmail,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                return new PaymentInitiationResult
                {
                    Success = true,
                    GatewayTransactionId = intent.Id,
                    ClientSecret = intent.ClientSecret,
                    PublishableKey = _settings.PublishableKey
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe PaymentIntent creation failed for booking {BookingId}", request.BookingId);
                return PaymentInitiationResult.Fail($"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(
            string gatewayTransactionId,
            string? additionalData = null)
        {
            try
            {
                var service = new PaymentIntentService();
                var intent = await service.GetAsync(gatewayTransactionId);

                return intent.Status switch
                {
                    "succeeded" => PaymentVerificationResult.Ok(intent.Id),
                    "processing" => new PaymentVerificationResult
                    {
                        Success = true,
                        Status = AppPaymentStatus.Processing,
                        GatewayTransactionId = intent.Id
                    },
                    _ => PaymentVerificationResult.Fail($"Payment not completed. Status: {intent.Status}")
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe verification failed for intent {IntentId}", gatewayTransactionId);
                return PaymentVerificationResult.Fail($"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
            }
        }

        public Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signature,
                    _settings.WebhookSecret);

                var result = stripeEvent.Type switch
                {
                    EventTypes.PaymentIntentSucceeded =>
                        WebhookProcessingResult.Ok(
                            AppPaymentStatus.Completed,
                            ((PaymentIntent)stripeEvent.Data.Object).Id),

                    EventTypes.PaymentIntentPaymentFailed =>
                        WebhookProcessingResult.Ok(
                            AppPaymentStatus.Failed,
                            ((PaymentIntent)stripeEvent.Data.Object).Id),

                    _ => WebhookProcessingResult.Ok(AppPaymentStatus.Pending, string.Empty)
                };

                return Task.FromResult(result);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook processing failed");
                return Task.FromResult(WebhookProcessingResult.Fail($"Webhook error: {ex.Message}"));
            }
        }

        public async Task<RefundResult> RefundAsync(
            string gatewayTransactionId,
            decimal amount,
            string reason)
        {
            try
            {
                // Retrieve the PaymentIntent to get the charge ID
                var intentService = new PaymentIntentService();
                var intent = await intentService.GetAsync(gatewayTransactionId);

                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = intent.Id,
                    Amount = (long)(amount * 100),
                    Reason = RefundReasons.RequestedByCustomer
                };

                var refundService = new RefundService();
                var refund = await refundService.CreateAsync(refundOptions);

                return RefundResult.Ok(refund.Id);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe refund failed for intent {IntentId}", gatewayTransactionId);
                return RefundResult.Fail($"Stripe refund error: {ex.StripeError?.Message ?? ex.Message}");
            }
        }
    }
}
