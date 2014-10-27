using System.Threading;

namespace Conesole
{
	class CircularQueue<T>
	{
		readonly T[] buffer;
		volatile int nextAvailable = 0;
		volatile int lastWritten = 0;
		volatile int read = 0;
	
		public CircularQueue(int bufferSize) {
			buffer = new T[bufferSize];
		}
	
		public void Enqueue(T value) {
			var claimed = Interlocked.Increment(ref nextAvailable) - 1;
			buffer[claimed % buffer.Length] = value;
			while(Interlocked.CompareExchange(ref lastWritten, claimed + 1, claimed) != claimed)
				Thread.SpinWait(1);
		}
	
		public bool TryDeque(out T value) {
			int claim, pos;
			for(;;) {
				pos = read;
				if(nextAvailable == pos) {
					value = default(T);
					return false;
				}
				claim = Interlocked.CompareExchange(ref read, pos + 1, pos);
				if(claim == pos) {
					value = buffer[claim % buffer.Length];
					return true;
				}
			}
		}
	}
}
