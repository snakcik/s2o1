using Microsoft.AspNetCore.Mvc;
using S2O1.Business.DTOs.Auth;
using S2O1.Business.Services.Interfaces;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _authService.LoginAsync(loginDto);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Token generation should be here or inside AuthService/TokenService
            // For now returning user info as proof of concept
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var user = await _authService.CreateUserAsync(createUserDto);
                return Ok(user);
            }
            catch (System.Exception ex)
            {
                // In production, don't expose internal exception details directly
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignUserRoleDto roleDto)
        {
             var result = await _authService.AssignRoleAsync(roleDto.UserId, roleDto.RoleId);
             if (result)
                 return Ok(new { message = "Role assigned successfully." });
             
             return BadRequest(new { message = "Failed to assign role. User or Role not found." });
        }
    }
}
