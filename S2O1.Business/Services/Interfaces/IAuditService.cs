using System.Threading.Tasks;

namespace S2O1.Business.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string actionType, string entityName, string entityId, string description);
    }
}
