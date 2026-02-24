using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceListController : ControllerBase
    {
        private readonly IPriceListService _priceListService;

        public PriceListController(IPriceListService priceListService)
        {
            _priceListService = priceListService;
        }

        [HttpGet]
        [Filters.Permission(new[] { "PriceList", "Offers" }, "Read")]
        public async Task<ActionResult<IEnumerable<PriceListDto>>> GetAll([FromQuery] string? status = null)
        {
            var data = await _priceListService.GetAllAsync(status);
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Filters.Permission(new[] { "PriceList", "Offers" }, "Read")]
        public async Task<ActionResult<PriceListDto>> GetById(int id)
        {
            var result = await _priceListService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Filters.Permission("PriceList", "Write")]
        public async Task<ActionResult<PriceListDto>> Create(CreatePriceListDto dto)
        {
            var result = await _priceListService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut]
        [Filters.Permission("PriceList", "Write")]
        public async Task<ActionResult<PriceListDto>> Update(UpdatePriceListDto dto)
        {
            var result = await _priceListService.UpdateAsync(dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Filters.Permission("PriceList", "Delete")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _priceListService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(result);
        }
    }
}
