namespace Cone.Core
{
	public interface ICallable
	{
		void Invoke(object obj, object[] parameters);
	}
}