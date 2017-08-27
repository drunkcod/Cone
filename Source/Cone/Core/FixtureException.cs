using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cone.Core
{
	[Serializable]
	public class FixtureException : Exception
	{
		readonly List<Exception> innerExceptions = new List<Exception>();
 
		public FixtureException(SerializationInfo info, StreamingContext context): base(info, context) { }

		internal FixtureException() { }

		public int Count { get { return innerExceptions.Count; } }
		public Exception this[int index]{ get { return innerExceptions[index]; } }

		internal void Add(Exception ex) { innerExceptions.Add(ex); }
	}
}