namespace Cone
{
    public interface IFormatter<T>
    {
        string Format(T expression);
    }
}
