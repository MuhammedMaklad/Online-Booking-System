using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Online_Booking_System.Contracts.Payment;
using Online_Booking_System.Models.Payments;
using Online_Booking_System.Settings;
using Online_Booking_System.ViewModels.Payment;

namespace Online_Booking_System.Services.Payment
{
    /// <summary>
    /// PayMob provider using the Accept Payments API (hosted iframe checkout).
    /// Flow: Authenticate → Create Order → Create Payment Key → Redirect to iframe.
    /// </summary>
    public class PayMobPaymentProvider : IPaymentProvider
    {
        private readonly PayMobSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PayMobPaymentProvider> _logger;

        public PaymentMethod Method => PaymentMethod.PayMob;

        public PayMobPaymentProvider(
            IOptions<PayMobSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<PayMobPaymentProvider> logger)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PayMob");

                // Step 1 — Authenticate and get token
                var authToken = await AuthenticateAsync(client);
                if (authToken is null)
                    return PaymentInitiationResult.Fail("PayMob authentication failed.");

                // Step 2 — Register order
                var orderId = await RegisterOrderAsync(client, authToken, request);
                if (orderId is null)
                    return PaymentInitiationResult.Fail("PayMob order registration failed.");

                // Step 3 — Obtain payment key
                var paymentKey = await GetPaymentKeyAsync(client, authToken, orderId, request);
                if (paymentKey is null)
                    return PaymentInitiationResult.Fail("PayMob payment key generation failed.");

                var redirectUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentKey}";

                return new PaymentInitiationResult
                {
                    Success = true,
                    GatewayTransactionId = orderId,
                    RedirectUrl = redirectUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayMob payment initiation failed for booking {BookingId}", request.BookingId);
                return PaymentInitiationResult.Fail($"PayMob error: {ex.Message}");
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(
            string gatewayTransactionId,
            string? additionalData = null)
        {
            // PayMob sends a callback; we verify the HMAC in the webhook handler.
            // Here we just trust the stored status from the webhook.
            if (additionalData == "success")
                return PaymentVerificationResult.Ok(gatewayTransactionId);

            return PaymentVerificationResult.Fail("Payment not confirmed by PayMob callback.");
        }

        public Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature)
        {
            try
            {
                // PayMob sends a query-string callback; payload is the concatenated HMAC string.
                var computedHmac = ComputeHmac(payload, _settings.HmacSecret);

                if (!string.Equals(computedHmac, signature, StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(WebhookProcessingResult.Fail("HMAC validation failed."));

                // Parse success flag from the payload (JSON or query-string)
                using var doc = JsonDocument.Parse(payload);
                var success = doc.RootElement.TryGetProperty("success", out var s) && s.GetBoolean();
                var transactionId = doc.RootElement.TryGetProperty("id", out var id)
                    ? id.ToString()
                    : string.Empty;

                var status = success ? PaymentStatus.Completed : PaymentStatus.Failed;
                return Task.FromResult(WebhookProcessingResult.Ok(status, transactionId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayMob webhook processing failed");
                return Task.FromResult(WebhookProcessingResult.Fail($"Webhook error: {ex.Message}"));
            }
        }

        public Task<RefundResult> RefundAsync(
            string gatewayTransactionId,
            decimal amount,
            string reason)
        {
            // PayMob refunds require a separate API call with the transaction ID.
            // Implementation left as a stub — wire up when needed.
            _logger.LogWarning("PayMob refund not yet implemented for transaction {Id}", gatewayTransactionId);
            return Task.FromResult(RefundResult.Fail("PayMob refunds are not yet configured."));
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<string?> AuthenticateAsync(HttpClient client)
        {
            var body = JsonSerializer.Serialize(new { api_key = _settings.ApiKey });
            var response = await client.PostAsync(
                $"{_settings.BaseUrl}/auth/tokens",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("token", out var t) ? t.GetString() : null;
        }

        private async Task<string?> RegisterOrderAsync(
            HttpClient client,
            string authToken,
            PaymentRequest request)
        {
            var body = JsonSerializer.Serialize(new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = (int)(request.Amount * 100),
                currency = request.Currency,
                merchant_order_id = request.TransactionId.ToString(),
                items = Array.Empty<object>()
            });

            var response = await client.PostAsync(
                $"{_settings.BaseUrl}/ecommerce/orders",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("id", out var id) ? id.ToString() : null;
        }

        private async Task<string?> GetPaymentKeyAsync(
            HttpClient client,
            string authToken,
            string orderId,
            PaymentRequest request)
        {
            var nameParts = request.UserFullName.Split(' ', 2);
            var body = JsonSerializer.Serialize(new
            {
                auth_token = authToken,
                amount_cents = (int)(request.Amount * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    apartment = "NA",
                    email = request.UserEmail,
                    floor = "NA",
                    first_name = nameParts.Length > 0 ? nameParts[0] : "Guest",
                    last_name = nameParts.Length > 1 ? nameParts[1] : "User",
                    street = "NA",
                    building = "NA",
                    phone_number = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "NA",
                    state = "NA"
                },
                currency = request.Currency,
                integration_id = int.Parse(_settings.IntegrationId)
            });

            var response = await client.PostAsync(
                $"{_settings.BaseUrl}/acceptance/payment_keys",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("token", out var t) ? t.GetString() : null;
        }

        private static string ComputeHmac(string data, string secret)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
