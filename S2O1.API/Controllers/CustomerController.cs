using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Business;
using S2O1.Business.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // --- Customer Company Endpoints ---

        [HttpGet("companies")]
        public async Task<ActionResult<IEnumerable<CustomerCompanyDto>>> GetAllCompanies()
        {
            var result = await _customerService.GetAllCompaniesAsync();
            return Ok(result);
        }

        [HttpGet("companies/{id}")]
        public async Task<ActionResult<CustomerCompanyDto>> GetCompanyById(int id)
        {
            var result = await _customerService.GetCompanyByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("companies")]
        public async Task<ActionResult<CustomerCompanyDto>> CreateCompany(CreateCustomerCompanyDto dto)
        {
            var result = await _customerService.CreateCompanyAsync(dto);
            return CreatedAtAction(nameof(GetCompanyById), new { id = result.Id }, result);
        }

        [HttpPut("companies")]
        public async Task<ActionResult<CustomerCompanyDto>> UpdateCompany(UpdateCustomerCompanyDto dto)
        {
            var result = await _customerService.UpdateCompanyAsync(dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("companies/{id}")]
        public async Task<ActionResult<bool>> DeleteCompany(int id)
        {
            var result = await _customerService.DeleteCompanyAsync(id);
            if (!result) return NotFound();
            return Ok(result);
        }

        // --- Customer Endpoints ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
        {
            var result = await _customerService.GetAllCustomersAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto dto)
        {
            var result = await _customerService.CreateCustomerAsync(dto);
            return CreatedAtAction(nameof(GetCustomerById), new { id = result.Id }, result);
        }

        [HttpPut]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(UpdateCustomerDto dto)
        {
            var result = await _customerService.UpdateCustomerAsync(dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteCustomer(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result) return NotFound();
            return Ok(result);
        }
    }
}
