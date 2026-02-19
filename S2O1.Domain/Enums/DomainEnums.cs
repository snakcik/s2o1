namespace S2O1.Domain.Enums
{
    public enum LicenseType
    {
        Demo = 1,
        Basic = 2,
        Expansion10 = 3,
        Expansion50 = 4,
        Expansion100 = 5
    }

    public enum MovementType : byte
    {
        Entry = 1,
        Exit = 2,
        Return = 3,
        Transfer = 4
    }

    public enum OfferStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Completed = 4
    }

    public enum InvoiceStatus
    {
        Draft = 1,
        Sent = 2,
        Paid = 3,
        Cancelled = 4,
        Approved = 5,
        WaitingForWarehouse = 6,
        InPreparation = 7,
        Delivered = 8
    }
}
