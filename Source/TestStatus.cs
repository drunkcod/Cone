using System;

namespace Cone
{
	[Flags]
	public enum TestStatus
	{
		ReadyToRun, 
		Success = 1,
		Pending = 1 << 1, 
		TestFailure = 1 << 2, 
		SetupFailure = 1 << 3, 
		TeardownFailure = 1 << 4
	}
}
