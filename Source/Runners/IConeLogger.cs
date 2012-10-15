using Cone.Core;
using System;
using System.IO;

namespace Cone.Runners
{
    public interface ISessionLogger
    {
        void BeginSession();
        void EndSession();
    }

    public class NullSessionLogger : ISessionLogger
    {
        public void BeginSession() { }
        public void EndSession() { }
    }

    public interface IConeLogger
    {
        void WriteInfo(Action<TextWriter> output);
        void Failure(ConeTestFailure failure);
        void Success(IConeTest test);
        void Pending(IConeTest test);
        void Skipped(IConeTest test);
    }
}
