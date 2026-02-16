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
        public async Task<IActionResult> GetLogs()
        {
             // Since we don't have a single "AuditLog" table populated yet, 
             // we will query recent modifications on key tables.
             // Or, if the user requested "AuditLogs" table, we check if it is populated.
             // Given the limited scope, let's look at Products, Warehouses as example.

             // Filter logic: Ignore changes by 'root'. Assuming 'root' has a specific ID (e.g. 1) or Name.
             // The user said "root kullanıcısının yaptığı değişiklikler gözükmeyecek".
             // We need to know who is root. Usually ID 1.

             var products = await _context.Products
                 .Where(p => !p.IsDeleted) // or include deleted if soft deleted
                 .OrderByDescending(p => p.CreateDate)
                 .Take(50)
                 .Select(p => new 
                 {
                     Entity = "Product",
                     Id = p.Id,
                     Name = p.ProductName,
                     UpdatedBy = p.UpdatedByUserId,
                     CreatedBy = p.CreatedByUserId,
                     Date = p.CreateDate
                 }).ToListAsync();

             // This is a naive implementation. Ideally we use AuditLogs table.
             // If we really want to capture ALL changes, we need Interceptor or AuditLog entries.
             // But for now, let's return what we have on entities.
             
             // Filter out 'Root' users
             var auditLogs = await _context.AuditLogs
                                .Where(a => a.ActorRole != "Root")
                                .OrderByDescending(a => a.CreateDate)
                                .Take(100)
                                .ToListAsync();

             return Ok(auditLogs);
        }
    }
}
