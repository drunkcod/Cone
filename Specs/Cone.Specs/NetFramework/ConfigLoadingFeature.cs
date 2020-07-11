using System.Configuration;
using CheckThat;


namespace Cone.NetFramework.Features
{
	[Feature("Assembly Config Loading")]
    public class ConfigLoadingFeature
    {
		public void appSettings_are_available() => Check.That(() => ConfigurationManager.AppSettings["key"] == "value");
    }
}
