using System.Threading.Tasks;

namespace S2O1.Business.Services.Interfaces
{
    public interface ILicenseService
    {
        Task<bool> ValidateLicenseAsync();
        string GetHardwareId();
        string GenerateLicenseRequest();
        Task InstallLicenseAsync(string licenseFilePath);
    }
}
