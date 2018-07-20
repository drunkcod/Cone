using System.Collections.Generic;

namespace Cone
{
	public interface IHaveCategories
	{
		IEnumerable<string> Categories { get; }
	}
}
