using System.ComponentModel.DataAnnotations;

namespace S2O1.Business.DTOs.Auth
{
    public class AssignUserRoleDto
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int RoleId { get; set; }
    }
}
