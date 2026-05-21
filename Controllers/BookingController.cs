using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts;
using Online_Booking_System.Data;
using System.Security.Claims;

namespace Online_Booking_System.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly AppDbContext _context;

        public BookingController(IBookingService bookingService, AppDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return View(bookings);
        }

        // ── GET: show confirmation page ───────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // ── POST: do the cancel ───────────────────────────────────────────────
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var result = await _bookingService.CancelBookingAsync(bookingId, userId);

            TempData[result ? "Success" : "Error"] = result
                ? "Booking cancelled successfully."
                : "Failed to cancel booking.";

            return RedirectToAction(nameof(MyBookings));
        }
    }
}