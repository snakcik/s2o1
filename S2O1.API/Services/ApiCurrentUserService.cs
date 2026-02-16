using S2O1.Core.Interfaces;
using System.Security.Claims;

namespace S2O1.API.Services
{
    public class ApiCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiCurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId 
        { 
            get 
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && int.TryParse(claim.Value, out int userId))
                {
                    return userId;
                }

                // Fallback to Header for non-authenticated internal requests
                if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-User-Id", out var headerId) == true)
                {
                    if (int.TryParse(headerId, out int hUserId))
                    {
                        return hUserId;
                    }
                }

                return null;
            }
        }

        public string UserName 
        {
            get
            {
                var name = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(name)) return name;
                
                if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-User-Name", out var hName) == true)
                    return hName;
                    
                return string.Empty;
            }
        }

        public string UserRole 
        {
            get
            {
                var role = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(role)) return role;
                
                if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-User-Role", out var hRole) == true)
                    return hRole;
                    
                return string.Empty;
            }
        }

        public string IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

        public string Source => "API";
    }
}
