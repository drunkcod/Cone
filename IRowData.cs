namespace Cone
{
    public interface IRowData
    {
        bool IsPending { get; }
        string Name { get; }
        object[] Parameters { get; }
    }
}
