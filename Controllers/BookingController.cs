using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Booking_System.Contracts;
using System.Security.Claims;

namespace Online_Booking_System.Controllers
{
    [Authorize] 
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Challenge(); 

            var bookings = await _bookingService.GetUserBookingsAsync(userId);

            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Challenge();

            var result = await _bookingService.CancelBookingAsync(bookingId, userId);

            if (result)
            {
                TempData["Success"] = "Booking cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to cancel booking.";
            }

            return RedirectToAction(nameof(MyBookings));
        }
    }
}