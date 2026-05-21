using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Contracts
{
    public interface IBookingService
    {
        Task<bool> CreateBookingAsync(
            CreateBookingViewModel model,
            string userId);
        Task<IEnumerable<MyBookingViewModel>> GetUserBookingsAsync(string userId);
        Task<bool> CancelBookingAsync(int bookingId, string userId);
        Task<bool> AdminCancelBookingAsync(int bookingId, string adminId);
    }
}