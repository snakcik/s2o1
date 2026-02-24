using System.Collections.Generic;

namespace S2O1.Business.DTOs.Business
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string SupplierCompanyName { get; set; }
        public string SupplierContactName { get; set; }
        public string SupplierContactMail { get; set; }
        public string SupplierAddress { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateSupplierDto
    {
        public string SupplierCompanyName { get; set; }
        public string SupplierContactName { get; set; }
        public string SupplierContactMail { get; set; }
        public string SupplierAddress { get; set; }
    }

    public class UpdateSupplierDto : CreateSupplierDto
    {
        public int Id { get; set; }
    }
}
