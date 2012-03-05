using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ConeTestFailure(int sequenceNumber, ITestName testName, Exception error, int skipFrames) {
            SequenceNumber = sequenceNumber;
            TestName = testName.Name;
            Context = testName.Context;
            var errorLocation = new StackTrace(error, skipFrames, true).GetFrame(0);
            File = errorLocation.GetFileName();
            Line = errorLocation.GetFileLineNumber();
            Column = errorLocation.GetFileColumnNumber();
            Message = error.Message;
        }

        public override string ToString() {
            return string.Format("{0}({1}:{2}) {3} - {4}: {5}", File, Line, Column, Context, TestName, Message);
        }

    } 
}
