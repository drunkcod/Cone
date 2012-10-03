using System;
using System.Diagnostics;

namespace Cone
{
    public class ConeTestFailure
    {
        public readonly int SequenceNumber;
        public readonly string File;
        public readonly int Line;
        public readonly int Column;
        public readonly string Context;
        public readonly string TestName;
        public readonly string Message;

        public ConeTestFailure(int sequenceNumber, ITestName testName, Exception error) {
            SequenceNumber = sequenceNumber;
            TestName = testName.Name;
            Context = testName.Context;
            var stackTrace= new StackTrace(error, 0, true);
			var errorLocation = stackTrace.GetFrame(stackTrace.FrameCount - 1);
			if(errorLocation != null) {
				File = errorLocation.GetFileName();
				Line = errorLocation.GetFileLineNumber();
				Column = errorLocation.GetFileColumnNumber();
			}
            Message = error.Message;
        }

        public override string ToString() {
            return string.Format("{0}({1}:{2}) {3}.{4}: {5}", File, Line, Column, Context, TestName, Message);
        }

    } 
}
