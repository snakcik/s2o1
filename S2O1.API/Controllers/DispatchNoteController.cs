using Microsoft.AspNetCore.Mvc;
using S2O1.Business.Services.Interfaces;
using S2O1.Business.DTOs.Logistic;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DispatchNoteController : ControllerBase
    {
        private readonly IDispatchNoteService _service;

        public DispatchNoteController(IDispatchNoteService service)
        {
            _service = service;
        }

        [HttpGet]
        [Filters.Permission("Warehouse", "Read")] 
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        [Filters.Permission("Warehouse", "Read")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _service.GetByIdAsync(id);
            if (res == null) return NotFound();
            return Ok(res);
        }

        [HttpPost]
        [Filters.Permission("Warehouse", "Write")]
        public async Task<IActionResult> Create([FromBody] CreateDispatchNoteDto dto)
        {
            return Ok(await _service.CreateAsync(dto));
        }

        [HttpPut("{id}/status")]
        [Filters.Permission("Warehouse", "Write")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            return Ok(await _service.UpdateStatusAsync(id, status));
        }
    }
}
