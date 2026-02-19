using S2O1.Domain.Common;
using System;
using System.Collections.Generic;

namespace S2O1.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string RoleName { get; set; }
        public ICollection<User> Users { get; set; }
    }

    public class Module : BaseEntity
    {
        public string ModuleName { get; set; }
        public ICollection<UserPermission> Permissions { get; set; }
    }

    public class Company : BaseEntity
    {
        public string CompanyName { get; set; }
        public bool AllowNegativeStock { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Warehouse> Warehouses { get; set; }
    }

    public class Title : BaseEntity
    {
        public string TitleName { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        
        public ICollection<TitlePermission> Permissions { get; set; } = new List<TitlePermission>();
    }

    public class User : BaseEntity
    {
        public int RoleId { get; set; }
        public Role Role { get; set; }
        
        public int? CreatedByUserId { get; set; }
        
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserMail { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserRegNo { get; set; }
        
        public int? CompanyId { get; set; }
        public Company Company { get; set; }
        
        public int? TitleId { get; set; }
        public Title Title { get; set; }
        
        public byte[]? UserPicture { get; set; } 
        
        public int AccessFailedCount { get; set; }
        public DateTime? LockoutEnd { get; set; }

        public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
        public ICollection<UserApiKey> ApiKeys { get; set; } = new List<UserApiKey>();
        
        public string? QuickActionsJson { get; set; } 
        
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
    }

    public class UserPermission : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int ModuleId { get; set; }
        public Module Module { get; set; }
        
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool IsFull { get; set; }
    }

    // NEW
    public class TitlePermission : BaseEntity
    {
        public int TitleId { get; set; }
        public Title Title { get; set; }
        
        public int ModuleId { get; set; }
        public Module Module { get; set; }
        
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool IsFull { get; set; }
    }

    public class UserApiKey : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        
        public string KeyName { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }
}
