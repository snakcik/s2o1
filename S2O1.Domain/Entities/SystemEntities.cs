using S2O1.Domain.Common;
using S2O1.Domain.Enums;
using System;

namespace S2O1.Domain.Entities
{
    public class SystemSetting : BaseEntity
    {
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string? LogoAscii { get; set; }
        public string? AppVersion { get; set; }
    }

    public class LicenseInfo : BaseEntity
    {
        public string LicenseKey { get; set; }
        public LicenseType LicenseType { get; set; }
        public int UserLimit { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsBypassed { get; set; } = false;
        public DateTime LastCheckDate { get; set; }
    }

    public class AuditLog : BaseEntity
    {
        public int? ActorUserId { get; set; }
        public string ActorUserName { get; set; } // Added for easier display
        public string ActorRole { get; set; } // Root, Admin, User, System
        public string ActionType { get; set; } // Create, Update, etc.
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string? EntityDisplay { get; set; } // Human readable name (e.g. Product Name)
        public string ActionDescription { get; set; }
        public string Source { get; set; } // CLI, API, System
        public string IPAddress { get; set; }
        public string? OldValues { get; set; } // JSON format
        public string? NewValues { get; set; } // JSON format
    }

    public class SystemQueueTask : BaseEntity
    {
        public string TaskType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty; // JSON
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        public int RetryCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }
}
