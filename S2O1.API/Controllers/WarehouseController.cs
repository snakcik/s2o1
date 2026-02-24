using Microsoft.AspNetCore.Mvc;
using S2O1.Business.Services.Interfaces;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // ===== DEPO PANEL (Warehouse) endpoints - Warehouse:Read yetkisi yeterli =====

        [HttpGet]
        [Filters.Permission(new[] { "Warehouse", "WarehouseManagement", "Offers", "Reports" }, "Read")]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? searchTerm = null)
        {
            var list = await _warehouseService.GetAllAsync(status, searchTerm);
            return Ok(list);
        }

        [HttpGet("shelves")]
        [Filters.Permission(new[] { "Warehouse", "WarehouseManagement", "Offers", "Reports" }, "Read")]
        public async Task<IActionResult> GetAllShelves([FromQuery] string? status = null)
        {
            var list = await _warehouseService.GetAllShelvesAsync(status);
            return Ok(list);
        }

        [HttpGet("{id}/shelves")]
        [Filters.Permission(new[] { "Warehouse", "WarehouseManagement", "Offers", "Reports" }, "Read")]
        public async Task<IActionResult> GetShelves(int id)
        {
            var list = await _warehouseService.GetShelvesAsync(id);
            return Ok(list);
        }

        // ===== DEPO YÖNETİMİ (WarehouseManagement) endpoints - Sadece WarehouseManagement yetkisi =====

        [HttpPost]
        [Filters.Permission("WarehouseManagement", "Write")]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
        {
            try
            {
                var created = await _warehouseService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Filters.Permission("WarehouseManagement", "Write")]
        public async Task<IActionResult> Update([FromBody] UpdateWarehouseDto dto)
        {
            var updated = await _warehouseService.UpdateAsync(dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Filters.Permission("WarehouseManagement", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _warehouseService.DeleteAsync(id);
            if (!res) return NotFound();
            return Ok();
        }

        [HttpPost("shelves")]
        [Filters.Permission("WarehouseManagement", "Write")]
        public async Task<IActionResult> CreateShelf([FromBody] CreateWarehouseShelfDto dto)
        {
            try
            {
               var created = await _warehouseService.CreateShelfAsync(dto);
               return Ok(created);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("shelves/{id}")]
        [Filters.Permission("WarehouseManagement", "Delete")]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            var res = await _warehouseService.DeleteShelfAsync(id);
            if (!res) return NotFound();
            return Ok();
        }
    }
}
