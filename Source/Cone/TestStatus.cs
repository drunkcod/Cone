using System;

namespace Cone
{
	[Flags]
	public enum TestStatus
	{
		ReadyToRun,
		Running = 1,
		Success = 1 << 1,
		Pending = 1 << 2, 
		TestFailure = 1 << 3, 
		SetupFailure = 1 << 4, 
		TeardownFailure = 1 << 5
	}
}
