using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Implementation
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public AuditService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task LogAsync(string actionType, string entityName, string entityId, string description)
        {
            var log = new AuditLog
            {
                ActorUserId = _currentUser.UserId,
                ActorRole = _currentUser.UserRole ?? "System",
                ActionType = actionType,
                EntityName = entityName,
                EntityId = entityId,
                ActionDescription = description ?? "-",
                Source = _currentUser.Source ?? "System",
                CreateDate = DateTime.UtcNow,
                IPAddress = _currentUser.IpAddress
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(log);
            // We do NOT call SaveChanges here usually if we want it in the same transaction as business logic.
            // But if called from a Service that calls SaveChanges, it will be saved.
            // Requirement 565: "Audit logs MUST be written within the same transaction as the business operation"
            // So we just AddAsync, and let the caller (Service) call SaveChanges.
        }
    }
}
