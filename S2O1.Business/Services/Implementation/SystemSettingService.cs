using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Implementation
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SystemSettingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemSetting> GetSettingsAsync()
        {
            var settings = (await _unitOfWork.Repository<SystemSetting>().GetAllAsync()).FirstOrDefault();
            if (settings == null)
            {
                // Should have been seeded, but fallback
                return new SystemSetting { LogoAscii = "S2O1", SettingValue = "Welcome" };
            }
            return settings;
        }
    }
}
