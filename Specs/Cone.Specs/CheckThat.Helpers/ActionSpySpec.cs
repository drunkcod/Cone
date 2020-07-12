using System;
using CheckThat;
using Cone;

namespace CheckThat.Helpers
{
	[Describe(typeof(ActionSpy))]
	public class ActionSpySpec 
	{
		public void spy() {
			var n = 0;
			var spy = new ActionSpy(() => ++n);
			Assume.That(() => n == 0);

			spy.Invoke();
			Check.That(() => n == 1);

			((Action)spy)();
			Check.That(() => n == 2);
		}

		public void assignment() { 
			var called = false;
			ActionSpy mySpy = new Action(() => called = true);

			mySpy.Invoke();
			Check.That(() => called, () => mySpy.HasBeenCalled);
		}

		[Context("of T")]
		public class ActioSpyOfTSpec
		{
			public void spy() {
				var n = 0;
				var spy = new ActionSpy<int>(x => n = x);

				spy.Invoke(1);
				Check.That(() =>n == 1);

				((Action<int>)spy)(2);
				Check.That(() => n == 2);		
			}
			
			public void check() {
				var spy = new ActionSpy<string>();

				spy.Invoke("Hello World!");

				spy.Check(x => x == "Hello World!");
			}
		}

		[Context("of T1,T2")]
		public class ActionSpyOfT1T2Spec
		{
			public void spy() {
				var a = 0;
				var b = string.Empty;
				var spy = new ActionSpy<int, string>((x, y) => { a = x; b = y; });

				spy.Invoke(1, "A");
				Check.That(() => a == 1, () => b == "A");

				((Action<int, string>)spy)(2, "B");
				Check.That(() => a == 2, () => b == "B");		
			}
			
			public void check() {
				var spy = new ActionSpy<int, string>();

				spy.Invoke(2, "Two");

				spy.Check((a, _) => a == 2, (_, b) => b == "Two");
			}
		}

		[Context("of T1,T2,T3")]
		public class ActionSpyOfT1T2T3Spec
		{
			public void spy() {
				var a = 0;
				var b = string.Empty;
				var c = false;
				var spy = new ActionSpy<int, string, bool>((x, y, z) => { a = x; b = y; c = z; });

				spy.Invoke(1, "A", true);
				Check.That(() => a == 1, () => b == "A", () => c);

				((Action<int, string, bool>)spy)(2, "B", false);
				Check.That(() => a == 2, () => b == "B", () => c == false);		
			}
			
			public void check() {
				var spy = new ActionSpy<int, string, bool>();

				spy.Invoke(2, "Two", true);

				spy.Check((a, _, __) => a == 2, (_, b, __) => b == "Two", (_, __, c) => c);
			}
		}

		[Context("of T1,T2,T3,T4")]
		public class ActionSpyOfT1T2T3T4Spec
		{
			public void spy() {
				char a,b,c,d;
				a = b = c = d = ' ';
				var spy = new ActionSpy<char, char, char, char>((x, y, z, w) => { a = x; b = y; c = z; d = w;});

				spy.Invoke('A', 'B', 'C', 'D');
				Check.That(() => a == 'A', () => b == 'B', () => c == 'C', () => d == 'D');

				((Action<char, char, char, char>)spy)('1', '2', '3', '4');
				Check.That(() => a == '1', () => b == '2', () => c == '3', () => d == '4');
			}
			
			public void check() {
				var spy = new ActionSpy<int, string, bool, char>();

				spy.Invoke(2, "Two", true, 'X');

				spy.Check((a, b, c, d) => a == 2, (a, b, c, d) => b == "Two", (a, b, c, d) => c, (a, b, c, d) => d == 'X');
			}
		}

	}
}