namespace S2O1.Domain.Common
{
    public interface IConcurrencyHandled
    {
        byte[] RowVersion { get; set; }
    }
}
