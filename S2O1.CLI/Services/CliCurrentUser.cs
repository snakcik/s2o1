using S2O1.Core.Interfaces;

namespace S2O1.CLI.Services
{
    public class CliCurrentUser : ICurrentUserService
    {
        public int? UserId { get; private set; }
        public string UserName { get; private set; }
        public string UserRole { get; private set; }
        public string Source => "CLI";
        public string IpAddress => "Localhost";
        public bool IsRoot => UserId == 1;

        public void SetUser(int userId, string userName, string role)
        {
            UserId = userId;
            UserName = userName;
            UserRole = role;
        }
        
        public void Clear()
        {
            UserId = null;
            UserName = null;
            UserRole = null;
        }
    }
}
