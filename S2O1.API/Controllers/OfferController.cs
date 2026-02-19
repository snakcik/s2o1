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
        private readonly IMailService _mailService;

        public OfferController(IOfferService offerService, IMailService mailService)
        {
            _offerService = offerService;
            _mailService = mailService;
        }

        [HttpPost("send-email")]
        [Filters.Permission("Offers", "Write")]
        public async Task<IActionResult> SendOfferByEmail([FromBody] SendOfferEmailDto dto)
        {
            if (string.IsNullOrEmpty(dto.ToEmail) || string.IsNullOrEmpty(dto.HtmlContent))
                return BadRequest("Email ve içerik zorunludur.");

            try
            {
                await _mailService.SendOfferEmailAsync(dto.ToEmail, dto.Subject ?? "S2O1 Teklif Formu", dto.HtmlContent, "Offer.pdf");
                return Ok(new { message = "Email başarıyla gönderildi." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = "Email gönderilemedi: " + ex.Message });
            }
        }

        [HttpGet]
        [Filters.Permission("Offers", "Read")]
        public async Task<ActionResult<IEnumerable<OfferDto>>> GetAll()
        {
            var offers = await _offerService.GetAllAsync();
            return Ok(offers);
        }

        [HttpGet("{id}")]
        [Filters.Permission("Offers", "Read")]
        public async Task<ActionResult<OfferDto>> GetById(int id)
        {
            var offer = await _offerService.GetByIdAsync(id);
            if (offer == null) return NotFound();
            return Ok(offer);
        }

        [HttpPost]
        [Filters.Permission("Offers", "Write")]
        public async Task<ActionResult<OfferDto>> Create(CreateOfferDto dto)
        {
            var result = await _offerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Filters.Permission("Offers", "Write")]
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
        [Filters.Permission("Offers", "Write")]
        public async Task<IActionResult> Approve(int id, [FromQuery] int userId)
        {
            await _offerService.ApproveOfferAsync(id, userId);
            return Ok();
        }

        [HttpPost("{id}/invoice")]
        [Filters.Permission("Offers", "Write")]
        public async Task<ActionResult<int>> CreateInvoice(int id, [FromQuery] int userId)
        {
            var invoiceId = await _offerService.CreateInvoiceFromOfferAsync(id, userId);
            return Ok(invoiceId);
        }

        [HttpDelete("{id}")]
        [Filters.Permission("Offers", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _offerService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
