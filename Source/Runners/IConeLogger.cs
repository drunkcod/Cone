using Cone.Core;

namespace Cone.Runners
{
    public interface IConeLogger
    {
		void BeginSession();
		void EndSession();

        void Info(string format, params object[] args);
        void Failure(ConeTestFailure failure);
        void Success(IConeTest test);
        void Pending(IConeTest test);
    }
}
