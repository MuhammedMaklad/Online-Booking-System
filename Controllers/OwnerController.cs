using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Booking_System.Contracts;
using Online_Booking_System.ViewModels;
using System.Security.Claims;

namespace Online_Booking_System.Controllers
{
    [Authorize(Roles = "Owner,Admin")]
    public class OwnerController : Controller
    {
        private readonly IOwnerService _ownerService;

        public OwnerController(IOwnerService ownerService)
        {
            _ownerService = ownerService;
        }

        // ─── Helper ───────────────────────────────────────────────────────────────

        private string? GetOwnerId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ─── Dashboard ────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var model = await _ownerService.GetDashboardAsync(ownerId);
            return View(model);
        }

        // ─── My Properties ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> MyProperties()
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var properties = await _ownerService.GetMyPropertiesAsync(ownerId);
            return View(properties);
        }

        // ─── Create Property ──────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult CreateProperty()
        {
            return View(new CreatePropertyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProperty(CreatePropertyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var propertyId = await _ownerService.CreatePropertyAsync(model, ownerId);

            TempData["Success"] = "Property created successfully.";
            return RedirectToAction(nameof(MyProperties));
        }

        // ─── Edit Property ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var model = await _ownerService.GetPropertyForEditAsync(id, ownerId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(EditPropertyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var result = await _ownerService.UpdatePropertyAsync(model, ownerId);
            if (!result)
            {
                TempData["Error"] = "Failed to update property.";
                return View(model);
            }

            TempData["Success"] = "Property updated successfully.";
            return RedirectToAction(nameof(MyProperties));
        }

        // ─── Delete Property ──────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var model = await _ownerService.GetPropertyForEditAsync(id, ownerId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost, ActionName("DeleteProperty")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePropertyConfirmed(int id)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var result = await _ownerService.DeletePropertyAsync(id, ownerId);
            if (!result)
            {
                TempData["Error"] = "Failed to delete property.";
            }
            else
            {
                TempData["Success"] = "Property deleted successfully.";
            }

            return RedirectToAction(nameof(MyProperties));
        }

        // ─── Property Bookings ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> PropertyBookings(int id)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var model = await _ownerService.GetPropertyBookingsAsync(id, ownerId);
            if (model == null) return NotFound();

            return View(model);
        }

        // ─── Confirm Booking ──────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int bookingId, int propertyId)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var result = await _ownerService.ConfirmBookingAsync(bookingId, ownerId);
            TempData[result ? "Success" : "Error"] = result
                ? "Booking confirmed successfully."
                : "Failed to confirm booking.";

            return RedirectToAction(nameof(PropertyBookings), new { id = propertyId });
        }

        // ─── Cancel Booking ───────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId, int propertyId)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var result = await _ownerService.CancelBookingAsync(bookingId, ownerId);
            TempData[result ? "Success" : "Error"] = result
                ? "Booking cancelled successfully."
                : "Failed to cancel booking.";

            return RedirectToAction(nameof(PropertyBookings), new { id = propertyId });
        }
    }
}
