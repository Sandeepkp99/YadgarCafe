namespace YadgarCafe.Application.DTOs.Customer
{
    public class CustomerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
