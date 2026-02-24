namespace S2O1.Core.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string UserName { get; }
        string UserRole { get; }
        string Source { get; }
        string IpAddress { get; }
        bool IsRoot { get; }
    }
}
