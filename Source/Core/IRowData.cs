namespace Cone.Core
{
    public interface IRowData
    {
        bool IsPending { get; }
        string DisplayAs { get; }
        object[] Parameters { get; }
		bool HasResult { get; }
		object Result { get; }
    }
}
