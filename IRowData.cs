namespace Cone
{
    public interface IRowData
    {
        bool IsPending { get; }
        string DisplayAs { get; }
        object[] Parameters { get; }
    }
}
