namespace CheckThat.Formatting
{
    public interface IFormatter<T>
    {
        string Format(T value);
    }
}
