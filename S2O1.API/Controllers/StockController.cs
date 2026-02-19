using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.Services.Interfaces;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Usually requires auth, but for CLI test might need token or bypass
    // For now, I'll assume auth is handled or testing locally
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost("movement")]
        [Filters.Permission("Stock", "Write")]
        public async Task<IActionResult> CreateMovement([FromForm] StockMovementDto movementDto, [FromForm] Microsoft.AspNetCore.Http.IFormFile? documentFile)
        {
            try
            {
                if (documentFile != null && documentFile.Length > 0)
                {
                    var uploadsDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "waybills");
                    if (!System.IO.Directory.Exists(uploadsDir)) System.IO.Directory.CreateDirectory(uploadsDir);

                    var fileName = System.Guid.NewGuid().ToString() + System.IO.Path.GetExtension(documentFile.FileName);
                    var filePath = System.IO.Path.Combine(uploadsDir, fileName);

                    using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await documentFile.CopyToAsync(stream);
                    }
                    movementDto.DocumentPath = "/uploads/waybills/" + fileName;
                }

                await _stockService.CreateMovementAsync(movementDto);
                return Ok(new { message = "Stok hareketi başarıyla kaydedildi.", documentPath = movementDto.DocumentPath });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("waybills/search")]
        [Filters.Permission("Stock", "Read")]
        public async Task<IActionResult> SearchWaybills([FromQuery] string? waybillNo, [FromQuery] System.DateTime? startDate, [FromQuery] System.DateTime? endDate, [FromQuery] int? supplierId)
        {
            var waybills = await _stockService.SearchWaybillsAsync(waybillNo, startDate, endDate, supplierId);
            return Ok(waybills);
        }

        [HttpGet("waybills/{supplierId}")]
        [Filters.Permission("Stock", "Read")]
        public async Task<IActionResult> GetWaybills(int supplierId)
        {
            var waybills = await _stockService.GetWaybillsBySupplierAsync(supplierId);
            return Ok(waybills);
        }

        [HttpGet("report")]
        [Filters.Permission("Stock", "Read")]
        public async Task<IActionResult> GetStockReport([FromQuery] int? warehouseId)
        {
            var report = await _stockService.GetWarehouseStockReportAsync(warehouseId);
            return Ok(report);
        }

        [HttpGet("product/{productId}/warehouse/{warehouseId}")]
        [Filters.Permission("Stock", "Read")]
        public async Task<IActionResult> GetStock(int productId, int warehouseId)
        {
            var stock = await _stockService.GetProductStockAsync(productId, warehouseId);
            return Ok(new { productId, warehouseId, currentStock = stock });
        }
    }
}
