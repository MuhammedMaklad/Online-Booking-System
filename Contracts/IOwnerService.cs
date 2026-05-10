using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Contracts
{
    public interface IOwnerService
    {
        /// <summary>Returns dashboard stats for the given owner.</summary>
        Task<OwnerDashboardViewModel> GetDashboardAsync(string ownerId);

        /// <summary>Returns all properties belonging to the owner.</summary>
        Task<IEnumerable<OwnerPropertySummaryViewModel>> GetMyPropertiesAsync(string ownerId);

        /// <summary>Creates a new property owned by the given user.</summary>
        Task<int> CreatePropertyAsync(CreatePropertyViewModel model, string ownerId);

        /// <summary>Returns a property for editing, only if it belongs to the owner.</summary>
        Task<EditPropertyViewModel?> GetPropertyForEditAsync(int propertyId, string ownerId);

        /// <summary>Updates a property, only if it belongs to the owner.</summary>
        Task<bool> UpdatePropertyAsync(EditPropertyViewModel model, string ownerId);

        /// <summary>Deletes a property, only if it belongs to the owner.</summary>
        Task<bool> DeletePropertyAsync(int propertyId, string ownerId);

        /// <summary>Returns all bookings for a specific property owned by the owner.</summary>
        Task<PropertyBookingsViewModel?> GetPropertyBookingsAsync(int propertyId, string ownerId);

        /// <summary>Confirms a booking for a property owned by the owner.</summary>
        Task<bool> ConfirmBookingAsync(int bookingId, string ownerId);

        /// <summary>Cancels a booking for a property owned by the owner.</summary>
        Task<bool> CancelBookingAsync(int bookingId, string ownerId);
    }
}
