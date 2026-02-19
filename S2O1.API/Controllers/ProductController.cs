using Microsoft.AspNetCore.Mvc;
using S2O1.Business.Services.Interfaces;
using S2O1.Business.DTOs.Stock;
using S2O1.DataAccess.Contexts;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        
        private readonly S2O1DbContext _context;
        private readonly S2O1.Core.Interfaces.ICurrentUserService _userService;

        public ProductController(IProductService productService, S2O1DbContext context, S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            _productService = productService;
            _context = context;
            _userService = userService;
        }

        private async Task<bool> HasPermissionAsync(string module, string type)
        {
            var userId = _userService.UserId;
            if (!userId.HasValue) return false;
            if (userId == 1) return true; // Root check

            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(
                Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(
                    _context.UserPermissions, p => p.Module),
                p => p.UserId == userId.Value && 
                     p.Module.ModuleName == module &&
                     (p.IsFull || 
                      (type == "Read" && p.CanRead) || 
                      (type == "Write" && p.CanWrite) || 
                      (type == "Delete" && p.CanDelete)));
        }

        [HttpPost]
        [Filters.Permission("Product", "Write")]
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
        [Filters.Permission("Product", "Read")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _productService.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpGet]
        [Filters.Permission("Product", "Read")]
        public async Task<IActionResult> GetAll()
        {
            var p = await _productService.GetAllAsync();
            return Ok(p);
        }

        [HttpPut]
        [Filters.Permission("Product", "Write")]
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
        [Filters.Permission("Product", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _productService.DeleteAsync(id);
            if(!res) return NotFound();
            return Ok();
        }

        [HttpGet("brands")]
        [Filters.Permission("Product", "Read")]
        public async Task<IActionResult> GetBrands()
        {
            var data = await _productService.GetAllBrandsAsync();
            return Ok(data);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            // Allow if has Category:Read OR Product:Read OR Product:Write
            // Used for dropdowns in Product Add/Edit
            bool canReadCategory = await HasPermissionAsync("Category", "Read");
            bool canReadProduct = await HasPermissionAsync("Product", "Read");
            bool canWriteProduct = await HasPermissionAsync("Product", "Write");

            if (!canReadCategory && !canReadProduct && !canWriteProduct)
            {
               return Forbid();
            }

            var data = await _productService.GetAllCategoriesAsync();
            return Ok(data);
        }

        [HttpGet("units")]
        [Filters.Permission("Product", "Read")]
        public async Task<IActionResult> GetUnits()
        {
            var data = await _productService.GetAllUnitsAsync();
            return Ok(data);
        }
        [HttpPost("brands")]
        [Filters.Permission("Product", "Write")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto dto)
        {
            var res = await _productService.CreateBrandAsync(dto);
            return Ok(res);
        }

        [HttpPost("categories")]
        [Filters.Permission("Category", "Write")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var res = await _productService.CreateCategoryAsync(dto);
            return Ok(res);
        }

        [HttpPost("units")]
        [Filters.Permission("Product", "Write")]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto dto)
        {
            var res = await _productService.CreateUnitAsync(dto);
            return Ok(res);
        }

        [HttpPut("brands")]
        [Filters.Permission("Product", "Write")]
        public async Task<IActionResult> UpdateBrand([FromBody] UpdateBrandDto dto)
        {
            var res = await _productService.UpdateBrandAsync(dto);
            if(res == null) return NotFound();
            return Ok(res);
        }

        [HttpDelete("brands/{id}")]
        [Filters.Permission("Product", "Delete")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
             var res = await _productService.DeleteBrandAsync(id);
             if(!res) return NotFound();
             return Ok();
        }

        [HttpPut("categories")]
        [Filters.Permission("Category", "Write")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            var res = await _productService.UpdateCategoryAsync(dto);
            if(res == null) return NotFound();
            return Ok(res);
        }

        [HttpDelete("categories/{id}")]
        [Filters.Permission("Category", "Delete")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var res = await _productService.DeleteCategoryAsync(id);
            if(!res) return NotFound();
            return Ok();
        }

        [HttpPut("units")]
        [Filters.Permission("Product", "Write")]
        public async Task<IActionResult> UpdateUnit([FromBody] UpdateUnitDto dto)
        {
            var res = await _productService.UpdateUnitAsync(dto);
             if(res == null) return NotFound();
            return Ok(res);
        }

        [HttpDelete("units/{id}")]
        [Filters.Permission("Product", "Delete")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var res = await _productService.DeleteUnitAsync(id);
            if(!res) return NotFound();
            return Ok();
        }
    }
}
