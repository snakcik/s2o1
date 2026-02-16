using System;

namespace S2O1.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int? UpdatedByUserId { get; set; }
        public int? CreatedByUserId { get; set; }
    }
}
