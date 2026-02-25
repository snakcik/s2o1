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

        [HttpGet("{userId}")]
        [Filters.Permission("Users", "Read")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı." });
            return Ok(user);
        }

        [HttpGet]
        [Filters.Permission(new[] { "Users", "Warehouse", "WarehouseManagement" }, "Read")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int? creatorId, [FromQuery] string? status = null, [FromQuery] string? module = null, [FromQuery] string? searchTerm = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _authService.GetAllUsersAsync(creatorId, status, module, searchTerm, page, pageSize);
            return Ok(users);
        }

        [HttpGet("modules")]
        [Filters.Permission("Users", "Read")]
        public async Task<IActionResult> GetModules()
        {
            var modules = await _authService.GetAllModulesAsync();
            return Ok(modules);
        }

        [HttpGet("roles")]
        [Filters.Permission("Users", "Read")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _authService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{userId}/permissions")]
        [Filters.Permission("Users", "Read")]
        public async Task<IActionResult> GetPermissions(int userId)
        {
            var perms = await _authService.GetUserPermissionsAsync(userId);
            return Ok(perms);
        }

        [HttpPost("{userId}/permissions")]
        [Filters.Permission("Users", "Write")]
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
        [Filters.Permission("Users", "Write")]
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
        [Filters.Permission("Users", "Write")]
        public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignUserRoleDto roleDto)
        {
             if (userId != roleDto.UserId) return BadRequest("User ID mismatch.");

             var result = await _authService.AssignRoleAsync(userId, roleDto.RoleId);
             if (result)
                 return Ok(new { message = "Role assigned successfully." });
             
             return BadRequest(new { message = "Failed to assign role." });
        }

        [HttpDelete("{userId}")]
        [Filters.Permission("Users", "Delete")]
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
        [Filters.Permission("Users", "Write")]
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
        
        [HttpPost("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword)) 
                return BadRequest(new { message = "Yeni şifre boş olamaz." });
            
            try
            {
                var result = await _authService.ChangePasswordAsync(userId, dto);
                if (result)
                    return Ok(new { message = "Şifre başarıyla değiştirildi." });
                
                return BadRequest(new { message = "Şifre değiştirilemedi." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is required.");
            
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _authService.ForgotPasswordAsync(dto.Email, baseUrl);
            
            // For security, don't tell if user exists. 
            // But for this project, let's be friendly.
            if (result) return Ok(new { message = "Sıfırlama bağlantısı e-posta adresinize gönderildi." });
            return BadRequest(new { message = "Bu e-posta adresine kayıtlı kullanıcı bulunamadı." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("Token and new password are required.");

            try 
            {
                var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
                if (result) return Ok(new { message = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz." });
                return BadRequest(new { message = "Geçersiz veya süresi dolmuş bağlantı." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Title Endpoints
        [HttpGet("titles")]
        [Filters.Permission("Users", "Read")]
        public async Task<IActionResult> GetAllTitles()
        {
            var titles = await _authService.GetAllTitlesAsync();
            return Ok(titles);
        }

        [HttpGet("titles/{id}")]
        [Filters.Permission("Users", "Read")]
        public async Task<IActionResult> GetTitleById(int id)
        {
            var title = await _authService.GetTitleByIdAsync(id);
            if (title == null) return NotFound();
            return Ok(title);
        }

        [HttpGet("titles/company/{companyId}")]
        [Filters.Permission("Users", "Read")]
        public async Task<IActionResult> GetTitlesByCompany(int companyId)
        {
            var titles = await _authService.GetTitlesByCompanyAsync(companyId);
            return Ok(titles);
        }

        [HttpPost("titles")]
        [Filters.Permission("Users", "Write")]
        public async Task<IActionResult> CreateTitle([FromBody] CreateTitleDto dto)
        {
            var title = await _authService.CreateTitleAsync(dto);
            return Ok(title);
        }

        [HttpPut("titles/{id}")]
        [Filters.Permission("Users", "Write")]
        public async Task<IActionResult> UpdateTitle(int id, [FromBody] CreateTitleDto dto)
        {
            try
            {
                var title = await _authService.UpdateTitleAsync(id, dto);
                return Ok(title);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("titles/{id}")]
        [Filters.Permission("Users", "Delete")]
        public async Task<IActionResult> DeleteTitle(int id)
        {
            var result = await _authService.DeleteTitleAsync(id);
            if (result) return Ok(new { message = "Ünvan/Bölüm silindi." });
            return NotFound(new { message = "Ünvan bulunamadı." });
        }

        [HttpGet("titles/{titleId}/permissions")]
        [Filters.Permission("Users", "Read")]
        public async Task<IActionResult> GetTitlePermissions(int titleId)
        {
            var perms = await _authService.GetTitlePermissionsAsync(titleId);
            return Ok(perms);
        }

        [HttpPost("titles/{titleId}/permissions")]
        [Filters.Permission("Users", "Write")]
        public async Task<IActionResult> SaveTitlePermissions(int titleId, [FromBody] System.Collections.Generic.List<TitlePermissionDto> permissions)
        {
            var result = await _authService.SaveTitlePermissionsAsync(titleId, permissions);
            if (result) return Ok(new { message = "Ünvan yetkileri kaydedildi." });
            return BadRequest(new { message = "Kaydedilemedi." });
        }
    }
}
