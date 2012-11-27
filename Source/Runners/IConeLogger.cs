using Cone.Core;
using System;
using System.IO;

namespace Cone.Runners
{
    public interface ISessionLogger
    {
        void WriteInfo(Action<TextWriter> output);
        void BeginSession();
        ISuiteLogger BeginSuite(IConeSuite suite);
        void EndSession();
    }

    public interface ISuiteLogger
    {
        ITestLogger BeginTest(IConeTest test);
        void Done();
    }

    public interface ITestLogger
    {
        void Failure(ConeTestFailure failure);
        void Success();
        void Pending();
        void Skipped();
    }
}
