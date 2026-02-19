using Microsoft.AspNetCore.Mvc;
using S2O1.DataAccess.Contexts;
using S2O1.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using S2O1.Core.Interfaces;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly S2O1DbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public LogsController(S2O1DbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        // GET: api/logs
        // Shows recent changes from all tables (Products, etc.)
        // Filters out 'root' user changes.
        [HttpGet]
        [Filters.Permission("Logs", "Read")]
        public async Task<IActionResult> GetLogs()
        {
             var userRole = _currentUserService.UserRole;
             var query = _context.AuditLogs.AsQueryable();

             // Anayasa Rule 426: Root user invisibility.
             // Only Root can see Root actions.
             if (string.IsNullOrEmpty(userRole) || !userRole.Equals("root", System.StringComparison.OrdinalIgnoreCase))
             {
                 query = query.Where(a => a.ActorRole != "root" && a.ActorRole != "Root");
             }

             var auditLogs = await query
                                .OrderByDescending(a => a.CreateDate)
                                .Take(200)
                                .ToListAsync();

             return Ok(auditLogs);
        }
    }
}
