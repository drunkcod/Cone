namespace Cone
{
	[Feature("row tests")]
	public class RowTestFeature
	{
		public enum MyEnum { Nothing }

		[Row(1)]
		public void support_int_to_enum_conversion(MyEnum value) { }
	}
}
