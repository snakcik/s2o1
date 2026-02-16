using System;

namespace S2O1.Business.DTOs.Auth
{
    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RegNo { get; set; }
        public int RoleId { get; set; }
        public int? CompanyId { get; set; }
        public bool IsActive { get; set; }
    }
}
