namespace S2O1.Business.DTOs.Business
{
    public class CustomerCompanyDto
    {
        public int Id { get; set; }
        public string CustomerCompanyName { get; set; }
        public string CustomerCompanyAddress { get; set; }
        public string CustomerCompanyMail { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateCustomerCompanyDto
    {
        public string CustomerCompanyName { get; set; }
        public string CustomerCompanyAddress { get; set; }
        public string CustomerCompanyMail { get; set; }
    }

    public class UpdateCustomerCompanyDto : CreateCustomerCompanyDto
    {
        public int Id { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public int CustomerCompanyId { get; set; }
        public string CustomerCompanyName { get; set; }
        public string CustomerContactPersonName { get; set; }
        public string CustomerContactPersonLastName { get; set; }
        public string CustomerContactPersonMobilPhone { get; set; }
        public string CustomerContactPersonMail { get; set; }
        public string FullName => $"{CustomerContactPersonName} {CustomerContactPersonLastName}";
        public bool IsDeleted { get; set; }
    }

    public class CreateCustomerDto
    {
        public int CustomerCompanyId { get; set; }
        public string CustomerContactPersonName { get; set; }
        public string CustomerContactPersonLastName { get; set; }
        public string CustomerContactPersonMobilPhone { get; set; }
        public string CustomerContactPersonMail { get; set; }
    }

    public class UpdateCustomerDto : CreateCustomerDto
    {
        public int Id { get; set; }
    }
}
