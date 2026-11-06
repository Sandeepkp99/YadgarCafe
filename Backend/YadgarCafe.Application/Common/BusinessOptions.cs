namespace YadgarCafe.Application.Common
{
    public class BusinessOptions
    {
        public const string SectionName = "Business";

        /// <summary>Time zone that decides what "today" and "this month" mean in sales reports.</summary>
        public string TimeZoneId { get; set; } = "Asia/Kolkata";
    }
}
