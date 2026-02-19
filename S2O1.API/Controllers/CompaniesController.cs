using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Auth;
using S2O1.Business.Services.Interfaces;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        [Filters.Permission("Companies", "Read")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _companyService.GetAllAsync();
            return Ok(list);
        }

        [HttpPost]
        [Filters.Permission("Companies", "Write")]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
        {
            try
            {
                var created = await _companyService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Filters.Permission("Companies", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _companyService.DeleteAsync(id);
            if(res) return Ok(new { message = "Company deleted." });
            return NotFound();
        }
    }
}
