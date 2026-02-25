using S2O1.Business.DTOs.Auth;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Security;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace S2O1.Business.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMailService _mailService;
        private readonly ICurrentUserService _currentUserService;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher, IMailService mailService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _mailService = mailService;
            _currentUserService = currentUserService;
        }

        public async Task<UserDto?> LoginAsync(LoginDto loginDto)
        {
            // 1. Check Root User (ID=1) - Special Case: Username is hashed!
            var rootUser = await _unitOfWork.Repository<User>().GetByIdAsync(1);
            if (rootUser != null)
            {
                // Root is now stored with plaintext username 'root'
                if (rootUser.UserName.Equals(loginDto.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    // It is root! Now check password (hashed)
                    if (_passwordHasher.VerifyPassword(loginDto.Password, rootUser.UserPassword))
                    {
                        var rootDto = _mapper.Map<UserDto>(rootUser);
                        var rootRole = await _unitOfWork.Repository<Role>().GetByIdAsync(rootUser.RoleId);
                        if (rootRole != null) rootDto.Role = rootRole.RoleName;
                        rootDto.RoleId = rootUser.RoleId;
                        rootDto.RegNo = rootUser.UserRegNo;
                        rootDto.QuickActionsJson = rootUser.QuickActionsJson;
                        rootDto.Permissions = (await GetUserPermissionsAsync(rootUser.Id)).ToList();
                        return rootDto;
                    }
                }
            }

            // 2. Normal User Check (Username is NOT hashed for others)
            var user = (await _unitOfWork.Repository<User>()
                .FindAsync(u => u.UserName == loginDto.UserName)).FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            // Check Password
            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.UserPassword))
            {
                 return null;
            }
            
            var userDto = _mapper.Map<UserDto>(user);
            
            // Get Role
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId);
            if (role != null) userDto.Role = role.RoleName;
            userDto.RoleId = user.RoleId;
            userDto.RegNo = user.UserRegNo;
            userDto.QuickActionsJson = user.QuickActionsJson;
            userDto.Permissions = (await GetUserPermissionsAsync(user.Id)).ToList();

            return userDto;
        }

        public Task<string> GenerateTokenAsync(UserDto user)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var existingUser = (await _unitOfWork.Repository<User>().FindAsync(u => u.UserName == dto.UserName)).FirstOrDefault();
            if (existingUser != null) throw new Exception("Username already exists.");

            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(dto.RoleId);
            
            if (role != null && role.RoleName.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Root rolüne sahip yeni bir kullanıcı oluşturulamaz.");
            }

            if (role == null) 
            {
                var roles = await _unitOfWork.Repository<Role>().GetAllAsync();
                role = roles.FirstOrDefault(r => r.RoleName == "User");
                if (role == null) throw new Exception("Invalid Role ID and no default 'User' role exists.");
            }

            var newUser = new User
            {
                UserName = dto.UserName,
                UserFirstName = dto.FirstName,
                UserLastName = dto.LastName,
                UserMail = dto.Email,
                UserRegNo = dto.RegNo ?? Guid.NewGuid().ToString().Substring(0, 8),
                RoleId = role.Id,
                CreatedByUserId = dto.CreatedByUserId,
                CompanyId = dto.CompanyId,
                TitleId = dto.TitleId,
                IsActive = true
            };

            // Check if strong password is required
            var forceStrongSetting = await _unitOfWork.Repository<SystemSetting>().FindAsync(s => s.SettingKey == "ForceStrongPassword");
            if (forceStrongSetting.FirstOrDefault()?.SettingValue == "true")
            {
                ValidatePasswordStrength(dto.Password);
            }

            newUser.UserPassword = _passwordHasher.HashPassword(dto.Password);

            await _unitOfWork.Repository<User>().AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            // If Admin, grant full permissions to all modules
            if (role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var modules = await _unitOfWork.Repository<Module>().GetAllAsync();
                foreach (var module in modules)
                {
                    await _unitOfWork.Repository<UserPermission>().AddAsync(new UserPermission
                    {
                        UserId = newUser.Id,
                        ModuleId = module.Id,
                        CanRead = true,
                        CanWrite = true,
                        CanDelete = true,
                        IsFull = true
                    });
                }
                await _unitOfWork.SaveChangesAsync();
            }
            // Apply Title Permissions if exists
            else if (newUser.TitleId.HasValue)
            {
                 var titlePerms = (await _unitOfWork.Repository<TitlePermission>().FindAsync(p => p.TitleId == newUser.TitleId.Value)).ToList();
                 if (titlePerms.Any())
                 {
                     foreach (var tp in titlePerms)
                     {
                         await _unitOfWork.Repository<UserPermission>().AddAsync(new UserPermission
                         {
                             UserId = newUser.Id,
                             ModuleId = tp.ModuleId,
                             CanRead = tp.CanRead,
                             CanWrite = tp.CanWrite,
                             CanDelete = tp.CanDelete,
                             IsFull = tp.IsFull
                         });
                    }
                    await _unitOfWork.SaveChangesAsync();
                 }
            }

            var result = _mapper.Map<UserDto>(newUser);
            result.Role = role.RoleName;
            return result;
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null) return false;

            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(roleId);
            if (role == null) return false;

            if (role.RoleName.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                return false; 
            }

            user.RoleId = roleId;
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<S2O1.Business.DTOs.Common.PagedResultDto<UserDto>> GetAllUsersAsync(int? currentUserId = null, string? status = null, string? requiredModule = null, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var canSeeDeleted = await CanSeeDeletedAsync();
            var query = _unitOfWork.Repository<User>().Query();

            if (canSeeDeleted)
            {
                query = query.IgnoreQueryFilters().Include(u => u.Title).Include(u => u.Role).Include(u => u.Permissions);
                if (status == "passive") query = query.Where(u => u.IsDeleted);
                else if (status == "all") query = query.Where(u => true);
                else query = query.Where(u => !u.IsDeleted);
            }
            else
            {
                query = query.Include(u => u.Title).Include(u => u.Role).Include(u => u.Permissions).Where(u => !u.IsDeleted);
            }

            if (currentUserId.HasValue)
            {
                query = query.Where(u => u.CreatedByUserId == currentUserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(u => u.UserName.ToLower().Contains(search) || u.UserFirstName.ToLower().Contains(search) || u.UserLastName.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var users = await query.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new System.Collections.Generic.List<UserDto>();
            var roles = await _unitOfWork.Repository<Role>().GetAllAsync();
            
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(requiredModule) && user.Id != 1)
                {
                    // Check if this user has access to the module
                    var perm = await _unitOfWork.Repository<UserPermission>().Query()
                        .Include(p => p.Module)
                        .FirstOrDefaultAsync(p => p.UserId == user.Id && p.Module.ModuleName.ToLower() == requiredModule.ToLower());
                    
                    if (perm == null || (!perm.CanRead && !perm.CanWrite && !perm.IsFull))
                    {
                        continue;
                    }
                }

                var dto = _mapper.Map<UserDto>(user);
                var role = roles.FirstOrDefault(r => r.Id == user.RoleId);
                dto.Role = role?.RoleName;
                dto.RoleId = user.RoleId;
                dto.RegNo = user.UserRegNo;
                dto.QuickActionsJson = user.QuickActionsJson;
                dtos.Add(dto);
            }

            return new S2O1.Business.DTOs.Common.PagedResultDto<UserDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<ModuleDto>> GetAllModulesAsync()
        {
            // Whitelist: Only modules that correspond to real API permission attributes.
            // Prevents showing duplicate/entity-noise modules like Offer/Offers, Invoice/Invoices.
            var validModules = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            {
                "Warehouse", "WarehouseManagement",
                "Stock", "Sales",
                "Product", "Category", "Brand",
                "Supplier", "Customer",
                "Offers", "Invoices",
                "PriceList",
                "Users", "Companies", "System", "Logs", "Reports",
                "Auth"
            };

            // Ensure special module exists in DB
            var existingMod = await _unitOfWork.Repository<Module>().Query().FirstOrDefaultAsync(m => m.ModuleName == "ShowDeletedItems");
            if (existingMod == null)
            {
                await _unitOfWork.Repository<Module>().AddAsync(new Module { ModuleName = "ShowDeletedItems", IsActive = true });
                await _unitOfWork.SaveChangesAsync();
            }

            // ADDED: Special permission only root can see/assign
            if (_currentUserService.IsRoot)
            {
                validModules.Add("ShowDeletedItems");
            }

            var modules = await _unitOfWork.Repository<Module>().GetAllAsync();
            return modules
                .Where(m => validModules.Contains(m.ModuleName))
                .Select(m => new ModuleDto { Id = m.Id, Name = m.ModuleName });
        }

        public async Task<IEnumerable<UserPermissionDto>> GetUserPermissionsAsync(int userId)
        {
            var allModules = await GetAllModulesAsync();
            var userPerms = await _unitOfWork.Repository<UserPermission>().FindAsync(p => p.UserId == userId);
            
            var result = new System.Collections.Generic.List<UserPermissionDto>();
            foreach(var mod in allModules)
            {
                var perm = userPerms.FirstOrDefault(p => p.ModuleId == mod.Id);
                result.Add(new UserPermissionDto
                {
                   ModuleId = mod.Id,
                   ModuleName = mod.Name,
                   CanRead = perm?.CanRead ?? false,
                   CanWrite = perm?.CanWrite ?? false,
                   CanDelete = perm?.CanDelete ?? false,
                   IsFull = perm?.IsFull ?? false
                });
            }
            return result;
        }

        public async Task<bool> SaveUserPermissionsAsync(int userId, IEnumerable<UserPermissionDto> permissions)
        {
            var repo = _unitOfWork.Repository<UserPermission>();
            var existingPermissions = (await repo.FindAsync(p => p.UserId == userId)).ToList();
            
            // PROTECT: Find the ShowDeletedItems module ID
            var showDeletedMod = await _unitOfWork.Repository<Module>().Query().FirstOrDefaultAsync(m => m.ModuleName == "ShowDeletedItems");

            foreach(var dto in permissions)
            {
                // PROTECT: Only root can change ShowDeletedItems permission
                if (showDeletedMod != null && dto.ModuleId == showDeletedMod.Id && !_currentUserService.IsRoot)
                    continue;

                if (dto.IsFull)
                {
                    dto.CanRead = true;
                    dto.CanWrite = true;
                    dto.CanDelete = true;
                }
                else if (dto.CanWrite || dto.CanDelete) 
                {
                    dto.CanRead = true;
                }
                
                if(!dto.CanRead && !dto.CanWrite && !dto.CanDelete && !dto.IsFull) 
                {
                    var toRemove = existingPermissions.FirstOrDefault(p => p.ModuleId == dto.ModuleId);
                    if(toRemove != null) repo.Remove(toRemove);
                    continue;
                }

                var existing = existingPermissions.FirstOrDefault(p => p.ModuleId == dto.ModuleId);
                if (existing != null)
                {
                    existing.CanRead = dto.CanRead;
                    existing.CanWrite = dto.CanWrite;
                    existing.CanDelete = dto.CanDelete;
                    existing.IsFull = dto.IsFull;
                }
                else
                {
                    await repo.AddAsync(new UserPermission
                    {
                        UserId = userId,
                        ModuleId = dto.ModuleId,
                        CanRead = dto.CanRead,
                        CanWrite = dto.CanWrite,
                        CanDelete = dto.CanDelete,
                        IsFull = dto.IsFull
                    });
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null) return false;

            if (user.Id == 1)
            {
                throw new InvalidOperationException("Root kullanıcısı silinemez.");
            }

            user.IsDeleted = true;
            user.IsActive = false;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            if (!string.Equals(user.UserMail, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                 var existing = (await _unitOfWork.Repository<User>().FindAsync(u => u.UserMail == dto.Email && u.Id != userId)).FirstOrDefault();
                 if (existing != null) throw new Exception("Email already exists.");
            }

            user.UserFirstName = dto.FirstName;
            user.UserLastName = dto.LastName;
            user.UserMail = dto.Email;
            user.UserRegNo = dto.RegNo;
            // user.TitleId = dto.TitleId; // Handled below with change detection
            user.IsActive = dto.IsActive;
            user.QuickActionsJson = dto.QuickActionsJson;
            
            if (dto.RoleId > 0 && user.RoleId != dto.RoleId)
            {
                 var newRole = await _unitOfWork.Repository<Role>().GetByIdAsync(dto.RoleId);
                 if (newRole == null) throw new Exception("Invalid Role ID.");
                 if (newRole.RoleName.Equals("root", StringComparison.OrdinalIgnoreCase)) 
                     throw new InvalidOperationException("Cannot assign Root role.");
                 user.RoleId = dto.RoleId;
            }

            bool titleChanged = false;
            // Check title change
            if (dto.TitleId != user.TitleId)
            {
                titleChanged = true;
                user.TitleId = dto.TitleId;
            }


            if (dto.CompanyId.HasValue)
            {
                var company = await _unitOfWork.Repository<Company>().GetByIdAsync(dto.CompanyId.Value);
                if (company == null) throw new Exception("Invalid Company ID.");
                user.CompanyId = dto.CompanyId;
            }
            else
            {
                user.CompanyId = null;
            }

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Apply Title Permissions if changed
            if (titleChanged && user.TitleId.HasValue)
            {
                 // Clear existing permissions
                 var existingPerms = await _unitOfWork.Repository<UserPermission>().FindAsync(p => p.UserId == userId);
                 foreach(var ep in existingPerms) _unitOfWork.Repository<UserPermission>().Remove(ep);
                 
                 var titlePerms = await _unitOfWork.Repository<TitlePermission>().FindAsync(p => p.TitleId == user.TitleId.Value);
                 foreach (var tp in titlePerms)
                 {
                     await _unitOfWork.Repository<UserPermission>().AddAsync(new UserPermission
                     {
                         UserId = userId,
                         ModuleId = tp.ModuleId,
                         CanRead = tp.CanRead,
                         CanWrite = tp.CanWrite,
                         CanDelete = tp.CanDelete,
                         IsFull = tp.IsFull
                     });
                 }
                 await _unitOfWork.SaveChangesAsync();
            }

            var result = _mapper.Map<UserDto>(user);
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId);
            result.Role = role?.RoleName;
            return result;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null) return false;

            // Verify old password
            if (!_passwordHasher.VerifyPassword(dto.OldPassword, user.UserPassword))
            {
                throw new Exception("Eski şifre hatalı.");
            }

            // Check if strong password is required
            var forceStrongSetting = await _unitOfWork.Repository<SystemSetting>().FindAsync(s => s.SettingKey == "ForceStrongPassword");
            if (forceStrongSetting.FirstOrDefault()?.SettingValue == "true")
            {
                ValidatePasswordStrength(dto.NewPassword);
            }

            // Set new password
            user.UserPassword = _passwordHasher.HashPassword(dto.NewPassword);
            
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null) return null;

            var dto = _mapper.Map<UserDto>(user);
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId);
            if (role != null) dto.Role = role.RoleName;
            dto.RoleId = user.RoleId;
            dto.RegNo = user.UserRegNo;
            dto.QuickActionsJson = user.QuickActionsJson;
            dto.Permissions = (await GetUserPermissionsAsync(user.Id)).ToList();
            // No password in DTO!
            return dto;
        }

        public async Task<bool> ForgotPasswordAsync(string email, string baseUrl)
        {
            var user = (await _unitOfWork.Repository<User>().FindAsync(u => u.UserMail == email)).FirstOrDefault();
            if (user == null) return false; // Don't reveal if user exists for security? Actually, the prompt asks for a reset mail to be sent.

            // Generate a secure token
            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.Now.AddHours(1);

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            var resetLink = $"{baseUrl.TrimEnd('/')}/reset-password.html?token={token}";
            
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                    <h2 style='color: #4f46e5;'>Şifre Sıfırlama İsteği</h2>
                    <p>S2O1 Sistemi için şifre sıfırlama talebinde bulundunuz.</p>
                    <p>Aşağıdaki butona tıklayarak yeni şifrenizi belirleyebilirsiniz. Bu bağlantı 1 saat süreyle geçerlidir.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #4f46e5; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Şifremi Sıfırla</a>
                    </div>
                    <p>Eğer bu ismi siz yapmadıysanız, lütfen bu e-postayı dikkate almayınız.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='font-size: 0.8rem; color: #777;'>Bu e-posta otomatik olarak gönderilmiştir. Lütfen cevaplamayınız.</p>
                </div>";

            await _mailService.SendEmailAsync(user.UserMail, "S2O1 - Şifre Sıfırlama", emailBody, true);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            if (string.IsNullOrEmpty(token)) return false;

            var user = (await _unitOfWork.Repository<User>().FindAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenExpires > DateTime.Now)).FirstOrDefault();
            if (user == null) return false;

            // Validate password strength if enabled
            var forceStrong = await _unitOfWork.Repository<SystemSetting>().FindAsync(s => s.SettingKey == "ForceStrongPassword");
            if (forceStrong.FirstOrDefault()?.SettingValue == "true")
            {
                ValidatePasswordStrength(newPassword);
            }

            user.UserPassword = _passwordHasher.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private void ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                throw new Exception("Şifre en az 6 karakter olmalıdır.");

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            if (!hasUpper || !hasLower || !hasDigit)
                throw new Exception("Şifreniz en az bir büyük harf, bir küçük harf ve bir rakam içermelidir.");
        }

        // Title Management
        public async Task<IEnumerable<TitleDto>> GetAllTitlesAsync()
        {
            var titles = await _unitOfWork.Repository<Title>().Query().OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<IEnumerable<TitleDto>>(titles);
        }

        public async Task<TitleDto> GetTitleByIdAsync(int id)
        {
            var title = await _unitOfWork.Repository<Title>().GetByIdAsync(id);
            return _mapper.Map<TitleDto>(title);
        }

        public async Task<IEnumerable<TitleDto>> GetTitlesByCompanyAsync(int companyId)
        {
            var titles = await _unitOfWork.Repository<Title>().FindAsync(t => t.CompanyId == companyId);
            return _mapper.Map<IEnumerable<TitleDto>>(titles);
        }

        public async Task<TitleDto> CreateTitleAsync(CreateTitleDto dto)
        {
            var title = _mapper.Map<Title>(dto);
            await _unitOfWork.Repository<Title>().AddAsync(title);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TitleDto>(title);
        }

        public async Task<TitleDto> UpdateTitleAsync(int id, CreateTitleDto dto)
        {
            var title = await _unitOfWork.Repository<Title>().GetByIdAsync(id);
            if (title == null) throw new Exception("Ünvan bulunamadı.");

            title.TitleName = dto.TitleName;
            title.CompanyId = dto.CompanyId;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TitleDto>(title);
        }

        public async Task<bool> DeleteTitleAsync(int id)
        {
            var title = await _unitOfWork.Repository<Title>().GetByIdAsync(id);
            if (title == null) return false;

            // Check if any user is using this title? 
            // Better to just soft delete via BaseEntity logic if handled, but here we just remove.
            _unitOfWork.Repository<Title>().Remove(title);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<TitlePermissionDto>> GetTitlePermissionsAsync(int titleId)
        {
            var allModules = await GetAllModulesAsync();
            var titlePerms = await _unitOfWork.Repository<TitlePermission>().FindAsync(p => p.TitleId == titleId);
            
            var result = new System.Collections.Generic.List<TitlePermissionDto>();
            foreach(var mod in allModules)
            {
                var perm = titlePerms.FirstOrDefault(p => p.ModuleId == mod.Id);
                result.Add(new TitlePermissionDto
                {
                   ModuleId = mod.Id,
                   ModuleName = mod.Name,
                   CanRead = perm?.CanRead ?? false,
                   CanWrite = perm?.CanWrite ?? false,
                   CanDelete = perm?.CanDelete ?? false,
                   IsFull = perm?.IsFull ?? false
                });
            }
            return result;
        }

        public async Task<bool> SaveTitlePermissionsAsync(int titleId, IEnumerable<TitlePermissionDto> permissions)
        {
            var repo = _unitOfWork.Repository<TitlePermission>();
            var existingPermissions = (await repo.FindAsync(p => p.TitleId == titleId)).ToList();
            
            foreach(var dto in permissions)
            {
                if (dto.IsFull) { dto.CanRead = true; dto.CanWrite = true; dto.CanDelete = true; }
                else if (dto.CanWrite || dto.CanDelete) { dto.CanRead = true; }
                
                if(!dto.CanRead && !dto.CanWrite && !dto.CanDelete && !dto.IsFull) 
                {
                    var toRemove = existingPermissions.FirstOrDefault(p => p.ModuleId == dto.ModuleId);
                    if(toRemove != null) repo.Remove(toRemove);
                    continue;
                }

                var existing = existingPermissions.FirstOrDefault(p => p.ModuleId == dto.ModuleId);
                if (existing != null)
                {
                    existing.CanRead = dto.CanRead;
                    existing.CanWrite = dto.CanWrite;
                    existing.CanDelete = dto.CanDelete;
                    existing.IsFull = dto.IsFull;
                }
                else
                {
                    await repo.AddAsync(new TitlePermission
                    {
                        TitleId = titleId,
                        ModuleId = dto.ModuleId,
                        CanRead = dto.CanRead,
                        CanWrite = dto.CanWrite,
                        CanDelete = dto.CanDelete,
                        IsFull = dto.IsFull
                    });
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<bool> CanSeeDeletedAsync()
        {
            if (_currentUserService.IsRoot) return true;
            return await _unitOfWork.Repository<UserPermission>().Query()
                .Include(p => p.Module)
                .AnyAsync(p => p.UserId == _currentUserService.UserId && 
                               p.Module.ModuleName == "ShowDeletedItems" && 
                               (p.CanRead || p.IsFull));
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _unitOfWork.Repository<Role>().GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }
    }
}
