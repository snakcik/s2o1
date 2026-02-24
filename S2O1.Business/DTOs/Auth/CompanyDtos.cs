
using System.ComponentModel.DataAnnotations;

namespace S2O1.Business.DTOs.Auth
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public bool AllowNegativeStock { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateCompanyDto
    {
        [Required]
        public string CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public bool AllowNegativeStock { get; set; }
    }
}
