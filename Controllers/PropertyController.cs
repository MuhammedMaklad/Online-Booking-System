using Microsoft.AspNetCore.Mvc;
using Online_Booking_System.Contracts;
using Online_Booking_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace Online_Booking_System.Controllers
{
    [Authorize]
    public class PropertyController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IBookingService _bookingService;

        public PropertyController(IPropertyService propertyService, IBookingService bookingService)
        {
            _propertyService = propertyService;
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index(PropertyFilterViewModel filter)
        {
            filter.Properties = await _propertyService.GetFilteredAsync(filter);

            return View(filter);
        }
        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);

            if (property == null)
                return NotFound();

            return View(property);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reserve(CreateBookingViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", new { id = model.PropertyId });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var result = await _bookingService.CreateBookingAsync(model, userId);

            if (!result)
            {
                TempData["Error"] = "Reservation failed.";
                return RedirectToAction("Details", new { id = model.PropertyId });
            }

            TempData["Success"] = "Reservation created successfully.";

            return RedirectToAction("MyBookings", "Booking");
        }
    }
}