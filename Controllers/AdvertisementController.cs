using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Data;
using Online_Booking_System.Models.Advertisements;
using Online_Booking_System.ViewModels;
using System.Security.Claims;

namespace Online_Booking_System.Controllers
{
    [Authorize(Roles = "Owner,Admin")]
    public class AdvertisementController : Controller
    {
        private readonly AppDbContext _context;

        public AdvertisementController(AppDbContext context)
        {
            _context = context;
        }

        private string? GetOwnerId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);


        [HttpGet]
        public async Task<IActionResult> MyAdvertisements()
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var ads = await _context.Advertisements
                .Where(a => a.OwnerId == ownerId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AdminAdvertisementListViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    ImageUrl = a.ImageUrl,
                    Status = a.Status.ToString(),
                    AdminNotes = a.AdminNotes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return View(ads);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateAdvertisementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAdvertisementViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var ad = new Advertisement
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                OwnerId = ownerId,
                Status = AdvertisementStatus.Pending,
                CreatedAt = DateTime.Now
            };

            // If a file was uploaded, save it to wwwroot/uploads and set ImageUrl
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                var fileName = $"ad_{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                ad.ImageUrl = $"/uploads/{fileName}";
            }

            _context.Advertisements.Add(ad);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Advertisement submitted! It will be reviewed by an admin.";
            return RedirectToAction(nameof(MyAdvertisements));
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var ad = await _context.Advertisements
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == ownerId);

            if (ad == null) return NotFound();

            var model = new CreateAdvertisementViewModel
            {
                Title = ad.Title,
                Description = ad.Description,
                ImageUrl = ad.ImageUrl
            };

            ViewBag.Ad = ad;
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var ad = await _context.Advertisements
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == ownerId);

            if (ad == null) return NotFound();

            
            var model = new CreateAdvertisementViewModel
            {
                Title = ad.Title,
                Description = ad.Description,
                ImageUrl = ad.ImageUrl
            };

            ViewBag.AdId = id;
            ViewBag.CurrentStatus = ad.Status.ToString();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateAdvertisementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AdId = id;
                return View(model);
            }

            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var ad = await _context.Advertisements
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == ownerId);

            if (ad == null) return NotFound();

            ad.Title = model.Title;
            ad.Description = model.Description;
            // If a new file uploaded, save and update URL
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                var fileName = $"ad_{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                ad.ImageUrl = $"/uploads/{fileName}";
            }
            else
            {
                ad.ImageUrl = model.ImageUrl;
            }

            ad.Status = AdvertisementStatus.Pending;
            ad.AdminNotes = null;
            ad.ReviewedAt = null;

            _context.Advertisements.Update(ad);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Advertisement updated and resubmitted for review.";
            return RedirectToAction(nameof(MyAdvertisements));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var ad = await _context.Advertisements
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == ownerId);

            if (ad == null) return NotFound();

            var model = new CreateAdvertisementViewModel
            {
                Title = ad.Title,
                Description = ad.Description,
                ImageUrl = ad.ImageUrl
            };

            ViewBag.AdId = id;
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ownerId = GetOwnerId();
            if (ownerId == null) return Challenge();

            var ad = await _context.Advertisements
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == ownerId);

            if (ad == null)
            {
                TempData["Error"] = "Advertisement not found.";
                return RedirectToAction(nameof(MyAdvertisements));
            }

            _context.Advertisements.Remove(ad);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Advertisement deleted successfully.";
            return RedirectToAction(nameof(MyAdvertisements));
        }
    }
}