
namespace Cone.Runners
{
    public interface IConeLogger
    {
        void Info(string format, params object[] args);
        void Failure(ConeTestFailure failure);
    }
}
