using System;
using System.Threading;

namespace Cone.Core
{
	public class CircularQueue<T>
	{
		readonly T[] buffer;
		public long nextAvailable = 0;
		public long head = -1;
		public long tail = 0;
	
		public CircularQueue(int bufferSize) {
			buffer = new T[bufferSize];
		}
	
		public bool TryEnqueue(T value) {
			long claimed;
			do {
				claimed = nextAvailable;
				if(buffer.Length <= (claimed - tail))
					return false;
			} while(Interlocked.CompareExchange(ref nextAvailable, claimed + 1, claimed) != claimed);

			buffer[claimed % buffer.Length] = value;
			
			var prev = claimed - 1;
			for(var spins = 1; 
				Interlocked.CompareExchange(ref head, claimed, prev) != prev; ++spins) {
				Thread.SpinWait(spins);
			}

			return true;
		}
	
		public bool TryDeque(out T value) {
			long pos;
			do {
				pos = tail;
				if(head < pos) {
					value = default(T);
					return false;
				}
			} while(Interlocked.CompareExchange(ref tail, pos + 1, pos) != pos);

			value = buffer[pos % buffer.Length];
			return true;
		}
	}
}
