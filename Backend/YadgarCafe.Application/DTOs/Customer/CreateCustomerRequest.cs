namespace YadgarCafe.Application.DTOs.Customer
{
    public class CreateCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}
