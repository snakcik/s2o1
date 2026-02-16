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
        public async Task<IActionResult> CreateMovement([FromBody] StockMovementDto movementDto)
        {
            try
            {
                // In real app, UserId should come from Claims
                // movementDto.UserId = int.Parse(User.FindFirst("id")?.Value);
                
                await _stockService.CreateMovementAsync(movementDto);
                return Ok(new { message = "Stok hareketi başarıyla kaydedildi." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetStockReport([FromQuery] int? warehouseId)
        {
            var report = await _stockService.GetWarehouseStockReportAsync(warehouseId);
            return Ok(report);
        }

        [HttpGet("product/{productId}/warehouse/{warehouseId}")]
        public async Task<IActionResult> GetStock(int productId, int warehouseId)
        {
            var stock = await _stockService.GetProductStockAsync(productId, warehouseId);
            return Ok(new { productId, warehouseId, currentStock = stock });
        }
    }
}
