using System;
using System.Linq.Expressions;
using System.Reflection;
using CheckThat;

namespace Cone.Core
{
	public class ValidDisplay
	{
		string value;

		public ValidDisplay(bool value) {
			this.value = value ? "Valid" : "Invalid";
		}

		public override string ToString() {
			return this.value;
		}
	}

	[Describe(typeof(ConeTestNamer))]
	public class ConeTestNamerSpec
	{
		ConeTestNamer TestNamer = new ConeTestNamer();

		string NameFor(MethodInfo method) => TestNamer.NameFor(new Invokable(method));
		string NameFor(MethodInfo method, object[] parameters, FormatString format) => TestNamer.NameFor(new Invokable(method), parameters, format);

		public void converts_underscores_to_whitespace(){
			var thisMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			Check.That(() => NameFor(thisMethod) == "converts underscores to whitespace");
		}

		[DisplayAs("can be renamed via attribute")]
		public void Renamed() {
			var thisMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			Check.That(() => NameFor(thisMethod) == "can be renamed via attribute");
		}

		[DisplayAs("", Heading = "override heading")]
		public void WithFunkyHeading() {
			var thisMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			Check.That(() => NameFor(thisMethod) == "override heading");       
		}

		void MyMethod<T>(T arg){}

		[Row("0x{0:x4}", 10)
		,Row("{0,3}", 10)
		,Row("{0,3:x2}", 10)]
		public void argument_formatting(string format, object value) {
			Expression<Action<int>> e = x => MyMethod(x);
			var myMethod = ((MethodCallExpression)(e.Body)).Method;

			Check.That(() => NameFor(myMethod, new object[]{ value }, new FormatString(format)) == string.Format(format, value));
		}

		public void formats_array_arguments() {
			Expression<Action<string[]>> e = x => MyMethod(x);
			var myMethod = ((MethodCallExpression)(e.Body)).Method;

			Check.That(() => NameFor(myMethod, new []{ new string[] {"A", "1" } }, new FormatString("{arg}")) == "new [] { \"A\", \"1\" }");
		}

		public void doesnt_mangle_unknown_parameter_names() {
			Expression<Action<int>> e = x => MyMethod(x);
			var myMethod = ((MethodCallExpression)(e.Body)).Method;

			Check.That(() => NameFor(myMethod, new object[] { 42 }, new FormatString("{theAnswer}")) == "{theAnswer}");
		}

		public void ignores_escaped_format() {
			Expression<Action<int>> e = x => MyMethod(x);
			var myMethod = ((MethodCallExpression)(e.Body)).Method;

			Check.That(() => NameFor(myMethod, new object[] { 42 }, new FormatString("{{arg}} {{0}}")) == "{{arg}} {{0}}");
		}

		[Row("{0:D}", MyEnum.Value, "0")
		,Row("{0:D}", MyFlags.Flag1 | MyFlags.Flag2, "3")
		,Row("flags", MyFlags.Flag1 | MyFlags.Flag2, "flags(MyFlags.Flag1 | MyFlags.Flag2)")
		,Row("{0}", null, "")
		,Row("not-a-format-string", null, "not-a-format-string(null)")]
		public void formatted(string format, object value, string expected) { 
			Expression<Action<object>> e = x => MyMethod(x);
			var myMethod = ((MethodCallExpression)(e.Body)).Method;

			Check.That(() => NameFor(myMethod, new object[]{ value }, new FormatString(format)) == expected);
		}

		void MyMethodWithDisplayClass([DisplayClass(typeof(ValidDisplay))]  bool isValid) { }

		public void obeys_DisplayClassAttribute() {
			Expression<Action<bool>> e = x => MyMethodWithDisplayClass(x);
			var target = ((MethodCallExpression)(e.Body)).Method;

			var format = new FormatString("{0}");
			Check.That(
				() => NameFor(target , new object[]{ true }, format) == "Valid",
				() => NameFor(target , new object[]{ false }, format) == "Invalid");
		}

		[DisplayAs("{b} op {a}")]
		void BinaryOp(int a, int b){ }

		public void display_as_extrapolated_heading() {
			Expression<Action<int,int>> e = (x, y) => BinaryOp(x, y);
			var target = ((MethodCallExpression)e.Body).Method;		
			Check.That(() => NameFor(target) == "b op a");
		}
	}
}
