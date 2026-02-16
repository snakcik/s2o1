using Microsoft.AspNetCore.Mvc;
using S2O1.Business.Services.Interfaces;
using S2O1.Business.DTOs.Stock;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
             try
             {
                 var created = await _productService.CreateAsync(dto);
                 return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
             }
             catch(System.Exception ex)
             {
                 return BadRequest(ex.Message);
             }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _productService.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var p = await _productService.GetAllAsync();
            return Ok(p);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProductDto dto)
        {
             try
             {
                 var res = await _productService.UpdateAsync(dto);
                 if(res == null) return NotFound();
                 return Ok(res);
             }
             catch(System.Exception ex)
             {
                 return BadRequest(ex.Message);
             }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _productService.DeleteAsync(id);
            if(!res) return NotFound();
            return Ok();
        }

        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var data = await _productService.GetAllBrandsAsync();
            return Ok(data);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var data = await _productService.GetAllCategoriesAsync();
            return Ok(data);
        }

        [HttpGet("units")]
        public async Task<IActionResult> GetUnits()
        {
            var data = await _productService.GetAllUnitsAsync();
            return Ok(data);
        }
        [HttpPost("brands")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto dto)
        {
            var res = await _productService.CreateBrandAsync(dto);
            return Ok(res);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var res = await _productService.CreateCategoryAsync(dto);
            return Ok(res);
        }

        [HttpPost("units")]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto dto)
        {
            var res = await _productService.CreateUnitAsync(dto);
            return Ok(res);
        }

        [HttpPut("brands")]
        public async Task<IActionResult> UpdateBrand([FromBody] UpdateBrandDto dto)
        {
            var res = await _productService.UpdateBrandAsync(dto);
            if(res == null) return NotFound();
            return Ok(res);
        }

        [HttpDelete("brands/{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var res = await _productService.DeleteBrandAsync(id);
            if(!res) return NotFound();
            return Ok();
        }

        [HttpPut("categories")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            var res = await _productService.UpdateCategoryAsync(dto);
             if(res == null) return NotFound();
            return Ok(res);
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var res = await _productService.DeleteCategoryAsync(id);
            if(!res) return NotFound();
            return Ok();
        }

        [HttpPut("units")]
        public async Task<IActionResult> UpdateUnit([FromBody] UpdateUnitDto dto)
        {
            var res = await _productService.UpdateUnitAsync(dto);
             if(res == null) return NotFound();
            return Ok(res);
        }

        [HttpDelete("units/{id}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var res = await _productService.DeleteUnitAsync(id);
            if(!res) return NotFound();
            return Ok();
        }
    }
}
