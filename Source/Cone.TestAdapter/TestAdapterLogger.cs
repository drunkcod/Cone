
using Cone.Core;
using Cone.Runners;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cone.TestAdapter
{
	public class TestAdapterEventArgs : EventArgs
	{
		public TestCase TestCase;
		public TimeSpan Duration;
		public string ErrorMessage;
		public string ErrorStackTrace;
	}

	class TestAdapterLogger : MarshalByRefObject, ICrossDomainLogger
	{
		readonly string source;
		readonly IMessageLogger logger;

		readonly ConcurrentDictionary<ConeTestName, KeyValuePair<TestCase, Stopwatch>> testCases = new ConcurrentDictionary<ConeTestName, KeyValuePair<TestCase, Stopwatch>>();

		public EventHandler<TestAdapterEventArgs> OnBeginTest; 
		public EventHandler<TestAdapterEventArgs> OnSuccess; 
		public EventHandler<TestAdapterEventArgs> OnFailure; 
		public EventHandler<TestAdapterEventArgs> OnPending; 

		public TestAdapterLogger(IMessageLogger logger, string source) {
			this.source = source;
			this.logger = logger;
		}

		public void Write(LogSeverity severity, string message) =>
			logger.SendMessage(ToTestMessageLevel(severity), message);

		static TestMessageLevel ToTestMessageLevel(LogSeverity severity) {
			if(severity >= LogSeverity.Warning)
				return TestMessageLevel.Warning;
			if(severity < LogSeverity.Notice)
				return TestMessageLevel.Informational;
			return TestMessageLevel.Warning;
		}

		public void Success(ConeTestName testCase) {
			if(testCases.TryRemove(testCase, out var found))
				OnSuccess?.Invoke(this, new TestAdapterEventArgs { TestCase = found.Key, Duration = found.Value.Elapsed }); 
		}
		public void Failure(ConeTestName testCase, string file, int line, int column, string message, string stackTrace) {
			if(!testCases.TryRemove(testCase, out var found))
				return;
			found.Key.LineNumber = line;
			found.Key.CodeFilePath = file;
			OnFailure?.Invoke(this, new TestAdapterEventArgs {
				TestCase = found.Key,
				Duration = found.Value.Elapsed,
				ErrorMessage = message,
				ErrorStackTrace = stackTrace,
			});
		}
		public void Pending(ConeTestName testCase, string reason) {
			if(testCases.TryRemove(testCase, out var found))
				OnPending?.Invoke(this, new TestAdapterEventArgs { TestCase = found.Key, Duration = found.Value.Elapsed  }); 
		}

		public void BeginTest(ConeTestName testCase) {
			var newTestCase = new TestCase(testCase.FullName, ConeTestExecutor.ExecutorUri, source) {
				DisplayName = testCase.Name,
			};
			newTestCase.Traits.Add("Context", testCase.Context);
			testCases.TryAdd(testCase, new KeyValuePair<TestCase, Stopwatch>(newTestCase, Stopwatch.StartNew()));
			OnBeginTest?.Invoke(this, new TestAdapterEventArgs { TestCase = newTestCase });			
		}
	}
}