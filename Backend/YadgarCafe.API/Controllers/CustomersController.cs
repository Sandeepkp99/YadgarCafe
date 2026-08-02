using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.DTOs.Customer;
using YadgarCafe.Application.Services.Interfaces;

namespace YadgarCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Get all customers
        /// </summary>
        /// <returns>List of all customers</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetAll()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving customers.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>Customer details</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CustomerResponse>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid customer ID." });

                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return NotFound(new { message = "Customer not found." });

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the customer.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get customer by phone number
        /// </summary>
        /// <param name="phoneNumber">Customer phone number</param>
        /// <returns>Customer details</returns>
        [HttpGet("phone/{phoneNumber}")]
        [AllowAnonymous]
        public async Task<ActionResult<CustomerResponse>> GetByPhone(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                    return BadRequest(new { message = "Phone number is required." });

                var customer = await _customerService.GetCustomerByPhoneAsync(phoneNumber);
                if (customer == null)
                    return NotFound(new { message = "Customer not found." });

                return Ok(customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the customer.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <param name="request">Customer creation details</param>
        /// <returns>Created customer</returns>
        [HttpPost]
        public async Task<ActionResult<CustomerResponse>> Create([FromBody] CreateCustomerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var customer = await _customerService.CreateCustomerAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the customer.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing customer
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="request">Updated customer details</param>
        /// <returns>Updated customer</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerResponse>> Update(Guid id, [FromBody] UpdateCustomerRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid customer ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var customer = await _customerService.UpdateCustomerAsync(id, request);
                if (customer == null)
                    return NotFound(new { message = "Customer not found." });

                return Ok(customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the customer.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid customer ID." });

                var result = await _customerService.DeleteCustomerAsync(id);
                if (!result)
                    return NotFound(new { message = "Customer not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the customer.", error = ex.Message });
            }
        }
    }
}
