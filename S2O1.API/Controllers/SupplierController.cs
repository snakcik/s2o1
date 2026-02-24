using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Business;
using S2O1.Business.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        [Filters.Permission("Supplier", "Read")]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll([FromQuery] string? status = null)
        {
            var suppliers = await _supplierService.GetAllAsync(status);
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        [Filters.Permission("Supplier", "Read")]
        public async Task<ActionResult<SupplierDto>> GetById(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null) return NotFound();
            return Ok(supplier);
        }

        [HttpPost]
        [Filters.Permission("Supplier", "Write")]
        public async Task<ActionResult<SupplierDto>> Create(CreateSupplierDto dto)
        {
            var result = await _supplierService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut]
        [Filters.Permission("Supplier", "Write")]
        public async Task<ActionResult<SupplierDto>> Update(UpdateSupplierDto dto)
        {
            var result = await _supplierService.UpdateAsync(dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Filters.Permission("Supplier", "Delete")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _supplierService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(result);
        }
    }
}
