using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Invoice;
using S2O1.Business.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/invoice")]
    [Route("api/invoices")]
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
        public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? searchTerm = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _invoiceService.GetAllAsync(status, searchTerm, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Filters.Permission(new[] { "Invoices", "Warehouse" }, "Read")]
        public async Task<ActionResult<InvoiceDto>> GetById(int id)
        {
            var result = await _invoiceService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("by-number/{number}")]
        [Filters.Permission(new[] { "Invoices", "Warehouse" }, "Read")]
        public async Task<ActionResult<InvoiceDto>> GetByNumber(string number)
        {
            var result = await _invoiceService.GetByNumberAsync(number);
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

        [HttpPost("{id}/reject")]
        [Filters.Permission("Invoices", "Write")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var result = await _invoiceService.RejectInvoiceAsync(id);
                if (!result) return BadRequest();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
            try
            {
                var result = await _invoiceService.CompleteDeliveryAsync(dto);
                return result ? Ok() : BadRequest();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/incomplete")]
        [Filters.Permission("Warehouse", "Write")]
        public async Task<IActionResult> MarkAsIncomplete(int id)
        {
            try
            {
                var result = await _invoiceService.MarkAsIncompleteAsync(id);
                return result ? Ok() : BadRequest();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/transfer")]
        [Filters.Permission("Warehouse", "Write")]
        public async Task<IActionResult> TransferJob(int id, [FromQuery] int toUserId)
        {
            var result = await _invoiceService.TransferJobAsync(id, toUserId);
            return result ? Ok() : BadRequest();
        }

        [HttpPost("{id}/unassign")]
        [Filters.Permission("Warehouse", "Write")]
        public async Task<IActionResult> UnassignJob(int id)
        {
            var result = await _invoiceService.UnassignJobAsync(id);
            return result ? Ok() : BadRequest();
        }
    }
}
