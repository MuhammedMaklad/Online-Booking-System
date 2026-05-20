using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Data;
using Online_Booking_System.Models.Advertisements;

namespace Online_Booking_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ads = await _context.Advertisements
                .Where(a => a.Status == AdvertisementStatus.Approved)
                .OrderByDescending(a => a.ReviewedAt)
                .Take(6)
                .ToListAsync();

            return View(ads);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Online_Booking_System.Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [Route("Home/StatusCode/{code}")]
        public IActionResult ErrorPage(int code)
        {
            return code switch
            {
                404 => View("Error404"),
                403 => View("Error403"),
                _ => View("Error")
            };
        }
    }
}