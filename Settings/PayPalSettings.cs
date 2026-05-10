namespace Online_Booking_System.Settings
{
    public class PayPalSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>sandbox | live</summary>
        public string Mode { get; set; } = "sandbox";

        public string Currency { get; set; } = "USD";

        public string BaseUrl => Mode == "live"
            ? "https://api-m.paypal.com"
            : "https://api-m.sandbox.paypal.com";
    }
}
