using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using S2O1.Core.Interfaces;
using S2O1.DataAccess.Contexts;
using S2O1.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace S2O1.API.Filters
{
    public class PermissionAttribute : ActionFilterAttribute
    {
        private readonly string[] _moduleNames;
        private readonly string _permissionType; // Read, Write, Delete

        // Single module constructor (backward compatible)
        public PermissionAttribute(string moduleName, string permissionType)
        {
            _moduleNames = new[] { moduleName };
            _permissionType = permissionType;
        }

        // Multiple modules constructor (OR logic - user needs permission in ANY of the modules)
        public PermissionAttribute(string[] moduleNames, string permissionType)
        {
            _moduleNames = moduleNames;
            _permissionType = permissionType;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<S2O1DbContext>();

            var userId = userService.UserId;
            if (!userId.HasValue)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Root user bypasses all checks
            if (userId == 1)
            {
                await next();
                return;
            }

            var hasPermission = await dbContext.UserPermissions
                .AnyAsync(p => p.UserId == userId.Value &&
                               _moduleNames.Any(name => name.ToLower() == p.Module.ModuleName.ToLower()) &&
                               (p.IsFull ||
                                (_permissionType == "Read" && p.CanRead) ||
                                (_permissionType == "Write" && p.CanWrite) ||
                                (_permissionType == "Delete" && p.CanDelete)));

            if (!hasPermission)
            {
                var moduleList = string.Join(", ", _moduleNames);
                // Debug info: Show which user and which modules failed
                context.Result = new ObjectResult(new { 
                    message = $"Erişim Engellendi. Modüller: [{moduleList}], Yetki: {_permissionType}, Kullanıcı ID: {userId.Value}. Lütfen yöneticinizle iletişime geçin." 
                }) 
                { 
                    StatusCode = 403 
                };
                return;
            }

            await next();
        }
    }
}
