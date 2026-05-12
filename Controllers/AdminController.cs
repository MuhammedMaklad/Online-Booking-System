using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Booking_System.Contracts;
using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService) => _adminService = adminService;

        public async Task<IActionResult> Dashboard()
        {
            var model = await _adminService.GetDashboardAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Properties(AdminPropertyFilterViewModel filter)
        {
            var model = await _adminService.GetPropertiesAsync(filter);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewProperty(int id)
        {
            var property = await _adminService.GetPropertyByIdAsync(id);
            if (property == null) return NotFound();
            ViewBag.Property = property;
            return View(new AdminReviewPropertyViewModel { PropertyId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewProperty(AdminReviewPropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Property = await _adminService.GetPropertyByIdAsync(model.PropertyId);
                return View(model);
            }
            bool result = model.Action == "Approve"
                ? await _adminService.ApprovePropertyAsync(model.PropertyId, model.AdminNotes)
                : await _adminService.RejectPropertyAsync(model.PropertyId, model.AdminNotes);

            TempData[result ? "Success" : "Error"] = result
                ? $"Property {model.Action.ToLower()}d successfully."
                : "Action failed.";
            return RedirectToAction(nameof(Properties));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var result = await _adminService.DeletePropertyAsync(id);
            TempData[result ? "Success" : "Error"] = result ? "Property deleted." : "Delete failed.";
            return RedirectToAction(nameof(Properties));
        }

        [HttpGet]
        public async Task<IActionResult> Advertisements(AdminAdvertisementFilterViewModel filter)
        {
            var model = await _adminService.GetAdvertisementsAsync(filter);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewAdvertisement(AdminReviewAdvertisementViewModel model)
        {
            bool result = model.Action == "Approve"
                ? await _adminService.ApproveAdvertisementAsync(model.AdvertisementId, model.AdminNotes)
                : await _adminService.RejectAdvertisementAsync(model.AdvertisementId, model.AdminNotes);

            TempData[result ? "Success" : "Error"] = result
                ? $"Advertisement {model.Action.ToLower()}d."
                : "Action failed.";
            return RedirectToAction(nameof(Advertisements));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdvertisement(int id)
        {
            var result = await _adminService.DeleteAdvertisementAsync(id);
            TempData[result ? "Success" : "Error"] = result ? "Advertisement deleted." : "Delete failed.";
            return RedirectToAction(nameof(Advertisements));
        }
    }
}