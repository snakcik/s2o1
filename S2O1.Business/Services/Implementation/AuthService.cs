using S2O1.Business.DTOs.Auth;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Security;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;

namespace S2O1.Business.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
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
                        return rootDto;
                    }
                }
            }

            // 2. Normal User Check (Username is NOT hashed for others)
            var user = (await _unitOfWork.Repository<User>()
                .FindAsync(u => u.UserName == loginDto.UserName)).FirstOrDefault();

            if (user == null)
                return null;

            // Check Password
            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.UserPassword))
                return null;
            
            var userDto = _mapper.Map<UserDto>(user);
            
            // Get Role
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId);
            if (role != null) userDto.Role = role.RoleName;

            return userDto;
        }

        public Task<string> GenerateTokenAsync(UserDto user)
        {
            // Placeholder for JWT or just return a session ID for CLI
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var existingUser = (await _unitOfWork.Repository<User>().FindAsync(u => u.UserName == dto.UserName)).FirstOrDefault();
            if (existingUser != null) throw new Exception("Username already exists.");

            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(dto.RoleId);
            
            // SECURITY CHECK: Prevent creating 'root' users
            if (role != null && role.RoleName.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Root rolüne sahip yeni bir kullanıcı oluşturulamaz.");
            }

            // Default to 'User' role (Id=2 usually) if invalid or not provided
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
                UserPassword = _passwordHasher.HashPassword(dto.Password),
                CompanyId = dto.CompanyId,
                IsActive = true
            };

            await _unitOfWork.Repository<User>().AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

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

            // SECURITY CHECK: Prevent assigning 'root' role
            if (role.RoleName.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                return false; // Or throw exception
            }

            user.RoleId = roleId;
            // No direct update method in generic repo usually, entity tracking handles it if fetched via EF
            // Assuming EF Core Change Tracking works since we fetched it.
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int? currentUserId = null)
        {
            IEnumerable<User> users;

            if (currentUserId.HasValue)
            {
                // Only users created by this user (excluding deleted)
                users = await _unitOfWork.Repository<User>().FindAsync(u => u.CreatedByUserId == currentUserId.Value && !u.IsDeleted);
            }
            else
            {
                // All users (excluding deleted)
                users = (await _unitOfWork.Repository<User>().GetAllAsync()).Where(u => !u.IsDeleted);
            }

            var dtos = new System.Collections.Generic.List<UserDto>();
            var roles = await _unitOfWork.Repository<Role>().GetAllAsync();
            
            foreach (var user in users)
            {
                var dto = _mapper.Map<UserDto>(user);
                var role = roles.FirstOrDefault(r => r.Id == user.RoleId);
                dto.Role = role?.RoleName;
                dtos.Add(dto);
            }
            return dtos;
        }

        public async Task<IEnumerable<ModuleDto>> GetAllModulesAsync()
        {
            var modules = await _unitOfWork.Repository<Module>().GetAllAsync();
            return modules.Select(m => new ModuleDto { Id = m.Id, Name = m.ModuleName });
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
                   CanDelete = perm?.CanDelete ?? false
                });
            }
            return result;
        }

        public async Task<bool> SaveUserPermissionsAsync(int userId, IEnumerable<UserPermissionDto> permissions)
        {
            var repo = _unitOfWork.Repository<UserPermission>();
            var existingPermissions = (await repo.FindAsync(p => p.UserId == userId)).ToList();
            
            // Process incoming permissions
            foreach(var dto in permissions)
            {
                // Logic: Write/Delete implies Read
                if (dto.CanWrite || dto.CanDelete) dto.CanRead = true;
                
                // Check if all false (no permission) -> Should be removed if exists
                if(!dto.CanRead && !dto.CanWrite && !dto.CanDelete) 
                {
                    var toRemove = existingPermissions.FirstOrDefault(p => p.ModuleId == dto.ModuleId);
                    if(toRemove != null) repo.Remove(toRemove);
                    continue;
                }

                var existing = existingPermissions.FirstOrDefault(p => p.ModuleId == dto.ModuleId);
                if (existing != null)
                {
                    // Update existing
                    existing.CanRead = dto.CanRead;
                    existing.CanWrite = dto.CanWrite;
                    existing.CanDelete = dto.CanDelete;
                    // repo.Update(existing); // Usually not needed if tracked, but explicit update is safer if repo supports it
                }
                else
                {
                    // Add new
                    await repo.AddAsync(new UserPermission
                    {
                        UserId = userId,
                        ModuleId = dto.ModuleId,
                        CanRead = dto.CanRead,
                        CanWrite = dto.CanWrite,
                        CanDelete = dto.CanDelete
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

            // SECURITY CHECK: Prevent deleting root user (ID=1)
            if (user.Id == 1)
            {
                throw new InvalidOperationException("Root kullanıcısı silinemez.");
            }

            // Soft Delete
            user.IsDeleted = true;
            user.IsActive = false;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            // Prevent editing Root if not Root? 
            // Rules say Root needs plaintext username verify etc.
            // For now, allow basic updates.
            
            // Check email uniqueness if changed
            if (!string.Equals(user.UserMail, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                 var existing = (await _unitOfWork.Repository<User>().FindAsync(u => u.UserMail == dto.Email && u.Id != userId)).FirstOrDefault();
                 if (existing != null) throw new Exception("Email already exists.");
            }

            user.UserFirstName = dto.FirstName;
            user.UserLastName = dto.LastName;
            user.UserMail = dto.Email;
            user.UserRegNo = dto.RegNo;
            user.IsActive = dto.IsActive;
            
            // Update Role
            if (user.RoleId != dto.RoleId)
            {
                 // Prevent assigning Root role
                 var newRole = await _unitOfWork.Repository<Role>().GetByIdAsync(dto.RoleId);
                 if (newRole == null) throw new Exception("Invalid Role ID.");
                 if (newRole.RoleName.Equals("root", StringComparison.OrdinalIgnoreCase)) 
                     throw new InvalidOperationException("Cannot assign Root role.");
                 user.RoleId = dto.RoleId;
            }

            // Update Company
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

            var result = _mapper.Map<UserDto>(user);
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId);
            result.Role = role?.RoleName;
            return result;
        }
    }
}
