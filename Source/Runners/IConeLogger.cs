using Cone.Core;
using System;
using System.IO;

namespace Cone.Runners
{
    public interface ISessionLogger
    {
        void WriteInfo(Action<TextWriter> output);
        void BeginSession();
        IConeLogger BeginTest(IConeTest test);
        void EndSession();
    }

    public interface IConeLogger
    {
        void Failure(ConeTestFailure failure);
        void Success();
        void Pending();
        void Skipped();
    }
}
