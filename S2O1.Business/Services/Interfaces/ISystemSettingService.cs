using S2O1.Domain.Entities;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Interfaces
{
    public interface ISystemSettingService
    {
        Task<SystemSetting> GetSettingsAsync();
    }
}
