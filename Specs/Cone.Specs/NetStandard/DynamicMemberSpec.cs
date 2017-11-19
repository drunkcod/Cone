namespace Cone.Platform.NetStandard.Specs
{
	[Describe(typeof(DynamicMember))]
    public class DynamicMemberSpec
    {
		class MyThing
		{
			public int SomeField;
			public string StringyProp {  get; set; }
		}

		public void can_read_fields() {
			var source = new MyThing {  SomeField = 3 };

			Check.With(() => source.GetType().GetField(nameof(MyThing.SomeField))).That(
				theField => (int)DynamicMember.GetValue(theField, source) == source.SomeField,
				theField => DynamicMember.GetValue(theField, source)  == theField.GetValue(source));
		}

		public void can_read_properties() {
			var source = new MyThing { StringyProp = "Hello World" };

			Check.With(() => source.GetType().GetProperty(nameof(MyThing.StringyProp))).That(
				theProp => (string)DynamicMember.GetValue(theProp, source) == source.StringyProp,
				theProp => DynamicMember.GetValue(theProp, source) == theProp.GetValue(source));
		}
	}
}
 