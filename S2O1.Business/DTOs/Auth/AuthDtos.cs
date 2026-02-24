namespace S2O1.Business.DTOs.Auth
{
    public class LoginDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RegNo { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? CompanyId { get; set; }
        public int? TitleId { get; set; }
        public string? TitleName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? QuickActionsJson { get; set; }
        public System.Collections.Generic.List<UserPermissionDto> Permissions { get; set; } = new();
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TitleDto
    {
        public int Id { get; set; }
        public string TitleName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }

    public class CreateTitleDto
    {
        public string TitleName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }
}
