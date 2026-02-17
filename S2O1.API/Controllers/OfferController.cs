using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfferController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OfferController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OfferDto>>> GetAll()
        {
            var offers = await _offerService.GetAllAsync();
            return Ok(offers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OfferDto>> GetById(int id)
        {
            var offer = await _offerService.GetByIdAsync(id);
            if (offer == null) return NotFound();
            return Ok(offer);
        }

        [HttpPost]
        public async Task<ActionResult<OfferDto>> Create(CreateOfferDto dto)
        {
            var result = await _offerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OfferDto>> Update(int id, [FromBody] CreateOfferDto dto)
        {
            try
            {
                var result = await _offerService.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id, [FromQuery] int userId)
        {
            await _offerService.ApproveOfferAsync(id, userId);
            return Ok();
        }

        [HttpPost("{id}/invoice")]
        public async Task<ActionResult<int>> CreateInvoice(int id, [FromQuery] int userId)
        {
            var invoiceId = await _offerService.CreateInvoiceFromOfferAsync(id, userId);
            return Ok(invoiceId);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _offerService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
