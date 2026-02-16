using S2O1.Business.Services.Interfaces;
using S2O1.Core.Hardware;
using S2O1.Core.Security;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using S2O1.Domain.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Implementation
{
    public class LicenseService : ILicenseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LicenseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string GetHardwareId()
        {
            return HardwareIdentifier.GetHardwareId();
        }

        public string GenerateLicenseRequest()
        {
            var hwId = GetHardwareId();
            return AesEncryption.Encrypt($"REQ|{hwId}|{DateTime.Now:O}");
        }

        public async Task InstallLicenseAsync(string licenseFilePath)
        {
            if (!File.Exists(licenseFilePath)) throw new FileNotFoundException("License file not found.");
            
            var encryptedContent = await File.ReadAllTextAsync(licenseFilePath);
            var decrypted = AesEncryption.Decrypt(encryptedContent);
            
            // Format: LIC|HardwareID|Type|UserLimit|ExpirationDate
            var parts = decrypted.Split('|');
            if (parts.Length < 5 || parts[0] != "LIC") throw new Exception("Invalid license file format.");

            var hwId = parts[1];
            if (hwId != GetHardwareId()) throw new Exception("License does not match this hardware.");

            var type = Enum.Parse<LicenseType>(parts[2]);
            var limit = int.Parse(parts[3]);
            var expDate = string.IsNullOrEmpty(parts[4]) ? (DateTime?)null : DateTime.Parse(parts[4]);

            // Save to DB
            var existing = (await _unitOfWork.Repository<LicenseInfo>().GetAllAsync()).FirstOrDefault();
            if (existing == null) existing = new LicenseInfo();

            existing.LicenseKey = encryptedContent.Substring(0, 20) + "..."; // Store hash or part?
            existing.LicenseType = type;
            existing.UserLimit = limit;
            existing.ExpirationDate = expDate;
            existing.LastCheckDate = DateTime.Now;

            if (existing.Id == 0) await _unitOfWork.Repository<LicenseInfo>().AddAsync(existing);
            else _unitOfWork.Repository<LicenseInfo>().Update(existing);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ValidateLicenseAsync()
        {
            var license = (await _unitOfWork.Repository<LicenseInfo>().GetAllAsync()).FirstOrDefault();
            if (license == null) return false;
            
            if (license.IsBypassed) return true; // Root bypass

            if (license.ExpirationDate.HasValue && license.ExpirationDate < DateTime.Now) return false;
            
            // Should also re-verify hardware ID if stored, but we typically trust the DB record until re-install?
            // "Sisteminin 'donanım sabiti' bulurken... her kullanıcı eklendiği zaman da lisansa bakacak"
            // We assume DB record is trusted source of truth for "Installed License".
            
            return true;
        }
    }
}
