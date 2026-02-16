using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Auth;
using S2O1.Business.Services.Interfaces;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int? creatorId)
        {
            var users = await _authService.GetAllUsersAsync(creatorId);
            return Ok(users);
        }

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules()
        {
            var modules = await _authService.GetAllModulesAsync();
            return Ok(modules);
        }

        [HttpGet("{userId}/permissions")]
        public async Task<IActionResult> GetPermissions(int userId)
        {
            var perms = await _authService.GetUserPermissionsAsync(userId);
            return Ok(perms);
        }

        [HttpPost("{userId}/permissions")]
        public async Task<IActionResult> SavePermissions(int userId, [FromBody] System.Collections.Generic.List<UserPermissionDto> permissions)
        {
            try
            {
                // Prevent editing root user's permissions
                if (userId == 1)
                {
                    return BadRequest(new { message = "Root kullanıcısının yetkileri değiştirilemez." });
                }

                // Note: Add logic to prevent assigning permissions to modules user doesn't have access to?
                // For now, trust the admin.
                var result = await _authService.SaveUserPermissionsAsync(userId, permissions);
                if(result) return Ok(new { message = "Yetkiler kaydedildi." });
                return BadRequest(new { message = "Yetkiler kaydedilemedi." });
            }
            catch (System.Exception ex)
            {
                // Return inner exception if available for more detail
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { message = $"Hata: {msg}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var user = await _authService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetAllUsers), new { id = user.Id }, user);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{userId}/role")]
        public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignUserRoleDto roleDto)
        {
             if (userId != roleDto.UserId) return BadRequest("User ID mismatch.");

             var result = await _authService.AssignRoleAsync(userId, roleDto.RoleId);
             if (result)
                 return Ok(new { message = "Role assigned successfully." });
             
             return BadRequest(new { message = "Failed to assign role." });
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var result = await _authService.DeleteUserAsync(userId);
                if (result)
                    return Ok(new { message = "Kullanıcı silindi." });
                
                return NotFound(new { message = "Kullanıcı bulunamadı." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var result = await _authService.UpdateUserAsync(userId, dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
