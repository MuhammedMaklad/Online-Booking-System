using System.Net.Http.Headers;
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
    /// PayPal provider using the Orders v2 REST API (hosted checkout).
    /// Flow: Create Order → Redirect user → Capture on return.
    /// </summary>
    public class PayPalPaymentProvider : IPaymentProvider
    {
        private readonly PayPalSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PayPalPaymentProvider> _logger;

        public PaymentMethod Method => PaymentMethod.PayPal;

        public PayPalPaymentProvider(
            IOptions<PayPalSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<PayPalPaymentProvider> logger)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PayPal");
                var accessToken = await GetAccessTokenAsync(client);

                if (accessToken is null)
                    return PaymentInitiationResult.Fail("PayPal authentication failed.");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var orderBody = JsonSerializer.Serialize(new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            reference_id = request.TransactionId.ToString(),
                            description = request.Description,
                            amount = new
                            {
                                currency_code = request.Currency.ToUpper(),
                                value = request.Amount.ToString("F2")
                            }
                        }
                    },
                    application_context = new
                    {
                        return_url = request.SuccessUrl,
                        cancel_url = request.CancelUrl,
                        brand_name = "Online Booking System",
                        user_action = "PAY_NOW"
                    }
                });

                var response = await client.PostAsync(
                    $"{_settings.BaseUrl}/v2/checkout/orders",
                    new StringContent(orderBody, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("PayPal order creation failed: {Error}", error);
                    return PaymentInitiationResult.Fail("PayPal order creation failed.");
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var orderId = doc.RootElement.GetProperty("id").GetString();
                var approveLink = doc.RootElement
                    .GetProperty("links")
                    .EnumerateArray()
                    .FirstOrDefault(l => l.GetProperty("rel").GetString() == "approve")
                    .GetProperty("href")
                    .GetString();

                return new PaymentInitiationResult
                {
                    Success = true,
                    GatewayTransactionId = orderId,
                    RedirectUrl = approveLink
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal payment initiation failed for booking {BookingId}", request.BookingId);
                return PaymentInitiationResult.Fail($"PayPal error: {ex.Message}");
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(
            string gatewayTransactionId,
            string? additionalData = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PayPal");
                var accessToken = await GetAccessTokenAsync(client);

                if (accessToken is null)
                    return PaymentVerificationResult.Fail("PayPal authentication failed.");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Capture the order
                var response = await client.PostAsync(
                    $"{_settings.BaseUrl}/v2/checkout/orders/{gatewayTransactionId}/capture",
                    new StringContent("{}", Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("PayPal capture failed: {Error}", error);
                    return PaymentVerificationResult.Fail("PayPal capture failed.");
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var status = doc.RootElement.GetProperty("status").GetString();

                return status == "COMPLETED"
                    ? PaymentVerificationResult.Ok(gatewayTransactionId)
                    : PaymentVerificationResult.Fail($"PayPal order status: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal verification failed for order {OrderId}", gatewayTransactionId);
                return PaymentVerificationResult.Fail($"PayPal error: {ex.Message}");
            }
        }

        public Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature)
        {
            // PayPal webhook verification requires calling their verify-webhook-signature API.
            // For now we parse the event type and return the appropriate status.
            try
            {
                using var doc = JsonDocument.Parse(payload);
                var eventType = doc.RootElement.GetProperty("event_type").GetString();
                var resourceId = doc.RootElement
                    .GetProperty("resource")
                    .GetProperty("id")
                    .GetString() ?? string.Empty;

                var status = eventType switch
                {
                    "PAYMENT.CAPTURE.COMPLETED" => PaymentStatus.Completed,
                    "PAYMENT.CAPTURE.DENIED"    => PaymentStatus.Failed,
                    "PAYMENT.CAPTURE.REFUNDED"  => PaymentStatus.Refunded,
                    _                           => PaymentStatus.Pending
                };

                return Task.FromResult(WebhookProcessingResult.Ok(status, resourceId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal webhook processing failed");
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
                var client = _httpClientFactory.CreateClient("PayPal");
                var accessToken = await GetAccessTokenAsync(client);

                if (accessToken is null)
                    return RefundResult.Fail("PayPal authentication failed.");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // gatewayTransactionId here is the capture ID, not the order ID
                var body = JsonSerializer.Serialize(new
                {
                    amount = new
                    {
                        value = amount.ToString("F2"),
                        currency_code = _settings.Currency.ToUpper()
                    },
                    note_to_payer = reason
                });

                var response = await client.PostAsync(
                    $"{_settings.BaseUrl}/v2/payments/captures/{gatewayTransactionId}/refund",
                    new StringContent(body, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("PayPal refund failed: {Error}", error);
                    return RefundResult.Fail("PayPal refund failed.");
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var refundId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

                return RefundResult.Ok(refundId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal refund failed for capture {CaptureId}", gatewayTransactionId);
                return RefundResult.Fail($"PayPal refund error: {ex.Message}");
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<string?> GetAccessTokenAsync(HttpClient client)
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v1/oauth2/token");
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            tokenRequest.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await client.SendAsync(tokenRequest);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("access_token", out var t) ? t.GetString() : null;
        }
    }
}
