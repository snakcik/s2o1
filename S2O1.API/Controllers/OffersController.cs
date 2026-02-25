using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using S2O1.Business.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        [HttpGet]
        [Filters.Permission("Offers", "Read")]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? searchTerm = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var offers = await _offerService.GetAllAsync(status, searchTerm, page, pageSize);
            return Ok(offers);
        }

        [HttpPost("{id}/approve")]
        [Filters.Permission("Offers", "Write")]
        public async Task<IActionResult> ApproveOffer(int id)
        {
            // Get user id from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            await _offerService.ApproveOfferAsync(id, userId);
            return Ok(new { Message = "Offer approved successfully." });
        }

        [HttpPost("{id}/create-invoice")]
        [Filters.Permission("Offers", "Write")]
        public async Task<IActionResult> CreateInvoice(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var invoiceId = await _offerService.CreateInvoiceFromOfferAsync(id, userId);
            return Ok(new { Message = "Invoice created from offer.", InvoiceId = invoiceId });
        }
    }
}
