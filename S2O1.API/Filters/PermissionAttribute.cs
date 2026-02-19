using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using S2O1.Core.Interfaces;
using S2O1.DataAccess.Contexts;
using S2O1.Domain.Entities;
using System.Threading.Tasks;

namespace S2O1.API.Filters
{
    public class PermissionAttribute : ActionFilterAttribute
    {
        private readonly string _moduleName;
        private readonly string _permissionType; // Read, Write, Delete

        public PermissionAttribute(string moduleName, string permissionType)
        {
            _moduleName = moduleName;
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
            // Root user has id 1 in this system
            if (userId == 1)
            {
                await next();
                return;
            }

            var hasPermission = await dbContext.UserPermissions
                .Include(p => p.Module)
                .AnyAsync(p => p.UserId == userId.Value && 
                               p.Module.ModuleName == _moduleName &&
                               (p.IsFull || 
                                (_permissionType == "Read" && p.CanRead) ||
                                (_permissionType == "Write" && p.CanWrite) ||
                                (_permissionType == "Delete" && p.CanDelete)));

            if (!hasPermission)
            {
                context.Result = new ObjectResult(new { message = $"'{_moduleName}' modülünde '{_permissionType}' yetkiniz bulunmamaktadır." }) 
                { 
                    StatusCode = 403 
                };
                return;
            }

            await next();
        }
    }
}
