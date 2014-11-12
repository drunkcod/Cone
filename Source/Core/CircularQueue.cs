using System;
using System.Threading;

namespace Cone.Core
{
	public class CircularQueue<T>
	{
		readonly T[] buffer;
		int nextAvailable = 0;
		int lastWritten = -1;
		int nextRead = 0;
	
		public CircularQueue(int bufferSize) {
			buffer = new T[bufferSize];
		}
	
		public bool TryEnqueue(T value) {
			int claimed;
			do {
				claimed = nextAvailable;
				if(buffer.Length <= (claimed - nextRead))
					return false;
			} while(Interlocked.CompareExchange(ref nextAvailable, claimed + 1, claimed) != claimed);

			buffer[claimed % buffer.Length] = value;
			
			for(int spins = 1, prev = claimed - 1; Interlocked.CompareExchange(ref lastWritten, claimed, prev) != prev; ++spins) {
				Thread.SpinWait(spins);
			}

			return true;
		}
	
		public bool TryDeque(out T value) {
			int pos;
			do {
				pos = nextRead;
				if(pos == nextAvailable) {
					value = default(T);
					return false;
				}
			} while(Interlocked.CompareExchange(ref nextRead, pos + 1, pos) != pos);

			value = buffer[pos % buffer.Length];
			return true;
		}
	}
}
