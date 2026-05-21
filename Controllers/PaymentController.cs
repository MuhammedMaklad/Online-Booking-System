using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Booking_System.Contracts.Payment;
using Online_Booking_System.Models.Payments;
using Online_Booking_System.ViewModels.Payment;
using System.Security.Claims;

namespace Online_Booking_System.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // ── GET /Payment/Choose?bookingId=5 ───────────────────────────────────
        /// <summary>Shows the payment method selection page.</summary>
        [HttpGet]
        public IActionResult Choose(int bookingId)
        {
            if (bookingId <= 0)
                return BadRequest();

            var model = new InitiatePaymentViewModel
            {
                BookingId = bookingId,
                Currency = "USD"
            };

            return View(model);
        }

        // ── POST /Payment/Initiate ─────────────────────────────────────────────
        /// <summary>Creates the payment transaction and redirects to the gateway.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate(InitiatePaymentViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Choose", model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Challenge();

            var result = await _paymentService.InitiatePaymentAsync(
                model.BookingId,
                userId,
                model.Method,
                model.Currency);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Choose), new { bookingId = model.BookingId });
            }

            // Stripe uses embedded checkout (client secret)
            if (model.Method == PaymentMethod.Stripe)
            {
                var checkoutVm = new PaymentCheckoutViewModel
                {
                    TransactionId = result.TransactionId,
                    BookingId = model.BookingId,
                    Amount = 0, // will be fetched from transaction
                    Currency = model.Currency,
                    Method = model.Method,
                    ClientSecret = result.ClientSecret,
                    PublishableKey = result.PublishableKey
                };

                // Fetch amount from the transaction we just created
                var txn = await _paymentService.GetTransactionByIdAsync(result.TransactionId);
                if (txn is not null)
                {
                    checkoutVm.Amount = txn.Amount;
                    checkoutVm.PropertyTitle = txn.PropertyTitle;
                }

                return View("~/Views/Payment/StripeCheckout.cshtml", checkoutVm);
            }

            // PayMob / PayPal use hosted redirect
            if (!string.IsNullOrEmpty(result.RedirectUrl))
                return Redirect(result.RedirectUrl);

            TempData["Error"] = "Unable to redirect to payment gateway.";
            return RedirectToAction(nameof(Choose), new { bookingId = model.BookingId });
        }

        // ── GET /Payment/Success ──────────────────────────────────────────────
        /// <summary>
        /// Called when the user returns from a hosted gateway (PayPal / PayMob).
        /// Also used as the Stripe post-payment redirect.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Success(
            int transactionId,
            string? token = null,          // PayPal order ID
            string? PayerID = null,        // PayPal payer
            string? order_id = null)       // PayMob order ID
        {
            // Determine the gateway transaction ID from query params
            var gatewayId = token ?? order_id ?? string.Empty;
            var additionalData = PayerID is not null ? "success" : null;

            var result = await _paymentService.VerifyPaymentAsync(transactionId, gatewayId, additionalData);

            if (result.Success)
            {
                TempData["Success"] = "Payment completed successfully! Your booking is confirmed.";
                return RedirectToAction("MyBookings", "Booking");
            }

            TempData["Error"] = $"Payment verification failed: {result.ErrorMessage}";
            return RedirectToAction(nameof(Failed), new { transactionId });
        }

        // ── POST /Payment/StripeConfirm ───────────────────────────────────────
        /// <summary>Called by Stripe.js after the PaymentIntent is confirmed client-side.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StripeConfirm(int transactionId, string paymentIntentId)
        {
            var result = await _paymentService.VerifyPaymentAsync(transactionId, paymentIntentId);

            if (result.Success)
            {
                TempData["Success"] = "Payment completed successfully! Your booking is confirmed.";
                return RedirectToAction("MyBookings", "Booking");
            }

            TempData["Error"] = $"Payment verification failed: {result.ErrorMessage}";
            return RedirectToAction(nameof(Failed), new { transactionId });
        }

        // ── GET /Payment/Cancel ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Cancel(int transactionId)
        {
            // Mark as cancelled via a failed verification
            await _paymentService.VerifyPaymentAsync(transactionId, string.Empty, "cancelled");
            TempData["Error"] = "Payment was cancelled.";
            return RedirectToAction("MyBookings", "Booking");
        }

        // ── GET /Payment/Failed ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Failed(int transactionId)
        {
            var txn = await _paymentService.GetTransactionByIdAsync(transactionId);
            return View(txn);
        }

        // ── GET /Payment/History?bookingId=5 ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> History(int bookingId)
        {
            IEnumerable<PaymentTransactionViewModel> transactions;

            // If admin and no bookingId specified, show all transactions
            if (User.IsInRole("Admin") && bookingId == 0)
            {
                transactions = await _paymentService.GetAllTransactionsAsync();
            }
            else
            {
                transactions = await _paymentService.GetTransactionsByBookingAsync(bookingId);
            }

            ViewBag.BookingId = bookingId;
            return View(transactions);
        }

        // ── POST /Payment/Webhook/{method} ────────────────────────────────────
        /// <summary>Receives webhook events from payment gateways. No [Authorize] — gateways call this.</summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("Payment/Webhook/{method}")]
        public async Task<IActionResult> Webhook(string method, [FromHeader(Name = "Stripe-Signature")] string? stripeSignature)
        {
            if (!Enum.TryParse<PaymentMethod>(method, ignoreCase: true, out var paymentMethod))
                return BadRequest("Unknown payment method.");

            string payload;
            using (var reader = new StreamReader(Request.Body))
            {
                payload = await reader.ReadToEndAsync();
            }

            var signature = stripeSignature
                ?? Request.Headers["X-PayMob-Hmac"].FirstOrDefault()
                ?? Request.Headers["PayPal-Transmission-Sig"].FirstOrDefault()
                ?? string.Empty;

            var result = await _paymentService.HandleWebhookAsync(paymentMethod, payload, signature);

            if (!result.Success)
            {
                _logger.LogWarning("Webhook processing failed for {Method}: {Error}", method, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok();
        }

        // ── GET /Payment/Refund/{id} ──────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(int id)
        {
            var transaction = await _paymentService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found.";
                return RedirectToAction(nameof(History));
            }

            var model = new RefundRequestViewModel();
            model.TransactionId = transaction.Id;
            model.PropertyTitle = transaction.PropertyTitle;
            model.Amount = transaction.Amount;
            model.Currency = transaction.Currency;

            return View(model);
        }

        // ── POST /Payment/Refund ──────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(RefundRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _paymentService.RefundAsync(model.TransactionId, model.Amount, model.Reason);

            if (result.Success)
            {
                TempData["Success"] = "Refund issued successfully.";
                return RedirectToAction(nameof(History));
            }

            TempData["Error"] = "Refund failed: " + result.ErrorMessage;
            return View(model);
        }
    }
}
