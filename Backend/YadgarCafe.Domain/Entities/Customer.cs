using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }

        /// <summary>Linked login account. Null for walk-in customers created by staff.</summary>
        public Guid? UserId { get; set; }

        public ICollection<Order> Orders { get; set; }
            = new List<Order>();
    }
}
