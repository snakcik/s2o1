using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Invoice;
using S2O1.Business.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        [Filters.Permission("Invoices", "Read")]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll()
        {
            var result = await _invoiceService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Filters.Permission("Invoices", "Read")]
        public async Task<ActionResult<InvoiceDto>> GetById(int id)
        {
            var result = await _invoiceService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Filters.Permission("Invoices", "Write")]
        public async Task<ActionResult<InvoiceDto>> Create(CreateInvoiceDto dto)
        {
            var result = await _invoiceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("{id}/approve")]
        [Filters.Permission("Invoices", "Write")]
        public async Task<IActionResult> Approve(int id, [FromQuery] int userId)
        {
            var result = await _invoiceService.ApproveInvoiceAsync(id, userId);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpGet("pending-deliveries")]
        [Filters.Permission("Warehouse", "Read")]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetPendingDeliveries()
        {
            var result = await _invoiceService.GetPendingDeliveriesAsync();
            return Ok(result);
        }

        [HttpPost("{id}/assign")]
        [Filters.Permission("Warehouse", "Write")]
        public async Task<IActionResult> Assign(int id, [FromQuery] int userId)
        {
            var result = await _invoiceService.AssignToDelivererAsync(id, userId);
            return result ? Ok() : BadRequest();
        }

        [HttpPost("complete-delivery")]
        [Filters.Permission("Warehouse", "Write")]
        public async Task<IActionResult> CompleteDelivery(WarehouseDeliveryDto dto)
        {
            var result = await _invoiceService.CompleteDeliveryAsync(dto);
            return result ? Ok() : BadRequest();
        }
    }
}
