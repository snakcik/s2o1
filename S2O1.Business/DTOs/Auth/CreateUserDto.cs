using System.ComponentModel.DataAnnotations;

namespace S2O1.Business.DTOs.Auth
{
    public class CreateUserDto
    {
        [Required]
        public string UserName { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        public string Email { get; set; }

        public string RegNo { get; set; } // Sicil No
        
        public int RoleId { get; set; } // Hangi role atanacak?

        public int? CreatedByUserId { get; set; } // Kim oluşturuyor? (Admin ID)
        public int? CompanyId { get; set; }
    }
}
