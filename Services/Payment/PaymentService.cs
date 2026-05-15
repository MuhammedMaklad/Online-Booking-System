using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts.Payment;
using Online_Booking_System.Data;
using Online_Booking_System.Models.Bookings;
using Online_Booking_System.Models.Payments;
using Online_Booking_System.ViewModels.Payment;

namespace Online_Booking_System.Services.Payment
{
    /// <summary>
    /// Orchestrates payment operations. Controllers talk only to this service.
    /// Delegates provider-specific work to the registered <see cref="IPaymentProvider"/> implementations.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IEnumerable<IPaymentProvider> _providers;
        private readonly ILogger<PaymentService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(
            AppDbContext context,
            IEnumerable<IPaymentProvider> providers,
            ILogger<PaymentService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _providers = providers;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // ── Initiate ──────────────────────────────────────────────────────────

        public async Task<PaymentInitiationResult> InitiatePaymentAsync(
            int bookingId,
            string userId,
            PaymentMethod method,
            string currency = "USD")
        {
            var booking = await _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking is null)
                return PaymentInitiationResult.Fail("Booking not found.");

            if (booking.Status == BookingStatus.Cancelled)
                return PaymentInitiationResult.Fail("Cannot pay for a cancelled booking.");

            // Prevent duplicate completed payments
            var existingCompleted = await _context.PaymentTransactions
                .AnyAsync(t => t.BookingId == bookingId && t.Status == PaymentStatus.Completed);

            if (existingCompleted)
                return PaymentInitiationResult.Fail("This booking has already been paid.");

            var provider = GetProvider(method);
            if (provider is null)
                return PaymentInitiationResult.Fail($"Payment method '{method}' is not supported.");

            // Create a pending transaction record first so we have an ID for the gateway
            var transaction = new PaymentTransaction
            {
                BookingId = bookingId,
                UserId = userId,
                Method = method,
                Amount = booking.TotalPrice,
                Currency = currency.ToUpper(),
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var request = BuildPaymentRequest(transaction, booking, currency);
            var result = await provider.InitiatePaymentAsync(request);

            if (!result.Success)
            {
                transaction.Status = PaymentStatus.Failed;
                transaction.FailureReason = result.ErrorMessage;
                transaction.FailedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return result;
            }

            // Persist gateway data
            transaction.GatewayTransactionId = result.GatewayTransactionId;
            transaction.Status = PaymentStatus.Processing;
            await _context.SaveChangesAsync();

            result.TransactionId = transaction.Id;
            return result;
        }

        // ── Verify ────────────────────────────────────────────────────────────

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(
            int transactionId,
            string gatewayTransactionId,
            string? additionalData = null)
        {
            var transaction = await _context.PaymentTransactions
                .Include(t => t.Booking)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction is null)
                return PaymentVerificationResult.Fail("Transaction not found.");

            if (transaction.Status == PaymentStatus.Completed)
                return new PaymentVerificationResult
                {
                    Success = true,
                    Status = PaymentStatus.Completed,
                    GatewayTransactionId = transaction.GatewayTransactionId
                };

            var provider = GetProvider(transaction.Method);
            if (provider is null)
                return PaymentVerificationResult.Fail("Provider not found.");

            var result = await provider.VerifyPaymentAsync(gatewayTransactionId, additionalData);

            transaction.Status = result.Status;
            transaction.GatewayTransactionId = gatewayTransactionId;

            if (result.Success && result.Status == PaymentStatus.Completed)
            {
                transaction.CompletedAt = DateTime.UtcNow;
                transaction.Booking.Status = BookingStatus.Confirmed;
            }
            else if (!result.Success)
            {
                transaction.FailureReason = result.ErrorMessage;
                transaction.FailedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        // ── Webhook ───────────────────────────────────────────────────────────

        public async Task<WebhookProcessingResult> HandleWebhookAsync(
            PaymentMethod method,
            string payload,
            string signature)
        {
            var provider = GetProvider(method);
            if (provider is null)
                return WebhookProcessingResult.Fail($"No provider for method '{method}'.");

            var result = await provider.ProcessWebhookAsync(payload, signature);

            if (!result.Success || result.NewStatus is null || string.IsNullOrEmpty(result.GatewayTransactionId))
                return result;

            // Find the transaction by gateway ID and update it
            var transaction = await _context.PaymentTransactions
                .Include(t => t.Booking)
                .FirstOrDefaultAsync(t => t.GatewayTransactionId == result.GatewayTransactionId);

            if (transaction is null)
            {
                _logger.LogWarning("Webhook received for unknown gateway transaction {Id}", result.GatewayTransactionId);
                return result;
            }

            transaction.Status = result.NewStatus.Value;

            switch (result.NewStatus.Value)
            {
                case PaymentStatus.Completed:
                    transaction.CompletedAt = DateTime.UtcNow;
                    transaction.Booking.Status = BookingStatus.Confirmed;
                    break;
                case PaymentStatus.Failed:
                    transaction.FailedAt = DateTime.UtcNow;
                    break;
                case PaymentStatus.Refunded:
                    transaction.Booking.Status = BookingStatus.Cancelled;
                    break;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        // ── Refund ────────────────────────────────────────────────────────────

        public async Task<RefundResult> RefundAsync(int transactionId, decimal amount, string reason)
        {
            var transaction = await _context.PaymentTransactions
                .Include(t => t.Booking)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction is null)
                return RefundResult.Fail("Transaction not found.");

            if (transaction.Status != PaymentStatus.Completed)
                return RefundResult.Fail("Only completed transactions can be refunded.");

            if (string.IsNullOrEmpty(transaction.GatewayTransactionId))
                return RefundResult.Fail("No gateway transaction ID on record.");

            var provider = GetProvider(transaction.Method);
            if (provider is null)
                return RefundResult.Fail("Provider not found.");

            var result = await provider.RefundAsync(transaction.GatewayTransactionId, amount, reason);

            if (result.Success)
            {
                transaction.Status = PaymentStatus.Refunded;
                transaction.Booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return result;
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public async Task<IEnumerable<PaymentTransactionViewModel>> GetTransactionsByBookingAsync(int bookingId)
        {
            return await _context.PaymentTransactions
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Property)
                .Where(t => t.BookingId == bookingId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => MapToViewModel(t))
                .ToListAsync();
        }

        public async Task<PaymentTransactionViewModel?> GetTransactionByIdAsync(int transactionId)
        {
            var t = await _context.PaymentTransactions
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Property)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            return t is null ? null : MapToViewModel(t);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private IPaymentProvider? GetProvider(PaymentMethod method) =>
            _providers.FirstOrDefault(p => p.Method == method);

        private PaymentRequest BuildPaymentRequest(
            PaymentTransaction transaction,
            Booking booking,
            string currency)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request is not null
                ? $"{request.Scheme}://{request.Host}"
                : string.Empty;

            return new PaymentRequest
            {
                BookingId = booking.Id,
                TransactionId = transaction.Id,
                UserId = booking.UserId,
                UserEmail = booking.User?.Email ?? string.Empty,
                UserFullName = booking.User?.FullName ?? "Guest",
                Amount = booking.TotalPrice,
                Currency = currency.ToUpper(),
                Description = $"Booking #{booking.Id} – {booking.Property?.Title}",
                SuccessUrl = $"{baseUrl}/Payment/Success?transactionId={transaction.Id}",
                CancelUrl = $"{baseUrl}/Payment/Cancel?transactionId={transaction.Id}",
                WebhookUrl = $"{baseUrl}/Payment/Webhook/{transaction.Method}"
            };
        }

        public async Task<IEnumerable<PaymentTransactionViewModel>> GetAllTransactionsAsync()
        {
            return await _context.PaymentTransactions
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Property)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => MapToViewModel(t))
                .ToListAsync();
        }

        private static PaymentTransactionViewModel MapToViewModel(PaymentTransaction t) =>
            new()
            {
                Id = t.Id,
                BookingId = t.BookingId,
                PropertyTitle = t.Booking?.Property?.Title ?? string.Empty,
                Method = t.Method,
                Amount = t.Amount,
                Currency = t.Currency,
                Status = t.Status,
                GatewayTransactionId = t.GatewayTransactionId,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt,
                FailureReason = t.FailureReason
            };
    }
}
