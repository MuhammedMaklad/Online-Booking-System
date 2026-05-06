using Online_Booking_System.Models.Properties;

namespace Online_Booking_System.ViewModels
{
    public class PropertyFilterViewModel
    {
        public string? SearchTerm { get; set; }

        public string? City { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }
        public int? Bedrooms { get; set; }

        public string? SortBy { get; set; }
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 8;

        public int TotalPages { get; set; }

        public IEnumerable<Property> Properties { get; set; }
            = new List<Property>();
    }
}