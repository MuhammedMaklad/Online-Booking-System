namespace Online_Booking_System.Settings
{
    public class PayMobSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string IntegrationId { get; set; } = string.Empty;
        public string IframeId { get; set; } = string.Empty;
        public string HmacSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://accept.paymob.com/api";
        public string Currency { get; set; } = "EGP";
    }
}
