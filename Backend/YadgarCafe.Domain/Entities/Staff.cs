using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    public class Staff : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Designation { get; set; } = string.Empty;
    }
}
