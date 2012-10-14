using Cone.Core;
using System;
using System.IO;

namespace Cone.Runners
{
    public interface IConeLogger
    {
		void BeginSession();
		void EndSession();

        void WriteInfo(Action<TextWriter> output);
        void Failure(ConeTestFailure failure);
        void Success(IConeTest test);
        void Pending(IConeTest test);
        void Skipped(IConeTest test);
    }
}
