namespace S2O1.Business.DTOs.Auth
{
    public class TitlePermissionDto
    {
        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool IsFull { get; set; }
    }
}
