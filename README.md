#Cone - ergonomic unit testing for .Net

```C#	
using Cone;

namespace WelcomeToCone
{
    [Describe(typeof(MyWidget))]
    public class MyWidgetSpec
    {
		public void the_widget_should_do_widgetty_things() {
			//Arrange some stuff.
			var theWidget = new MyWidget();
			//Do some Action
			theWidget.Frooble();

			Check.That(() => theWidget.HasBeenFroobled == true);
		}

        [Context("when the moon is cheese")]
        public class MyWidgetWhenTheMoonIsCheeseSpec
        {
            public void it_is_also_blue()//i.e. My Widget when the moon is cheese it is also blue
            {
                //TODO write test
                Check.That(() => theMoon != thatsNoMoon);
            }
        }
    }
}
```
