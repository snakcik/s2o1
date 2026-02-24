using System.ComponentModel.DataAnnotations;

namespace S2O1.Business.DTOs.Auth
{
    public class CreateUserDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public string Email { get; set; } = string.Empty;

        public string RegNo { get; set; } = string.Empty; // Sicil No
        
        public int RoleId { get; set; } // Hangi role atanacak?

        public int? CreatedByUserId { get; set; } // Kim oluşturuyor? (Admin ID)
        public int? CompanyId { get; set; }
        public int? TitleId { get; set; }
    }
}
