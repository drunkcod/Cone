using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cone.Runners
{
	public class ConeResolver : MarshalByRefObject
	{
		public const string ResolvePathsKey = "resolvePaths";
		struct ResolveCandidate
		{
			public ResolveCandidate(string path) {
				this.Path = path;
				this.Name = System.IO.Path.GetFileNameWithoutExtension(path);
			}
			
			public readonly string Path;
			public readonly string Name;
		}

		ResolveCandidate[] candidates;

		public ConeResolver() {
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
		}

		Assembly AssemblyResolve(object sender, ResolveEventArgs e) {
			if(candidates == null) {
				candidates = CandidateResolvePaths((string[])AppDomain.CurrentDomain.GetData(ResolvePathsKey));
				AppDomain.CurrentDomain.SetData(ResolvePathsKey, null);
			}
			var name = e.Name.Split(',')[0];

			var found = Array.FindIndex(candidates, x => x.Name == name);
			if(found != -1) {
				return Assembly.LoadFrom(candidates[found].Path);
			}

			return null;
		}
		
		static ResolveCandidate[] CandidateResolvePaths(string[] resolvePaths) {
			return 
				resolvePaths
				.SelectMany(x => Directory.GetFiles(x).Where(IsExeOrDll))
				.Select(x => new ResolveCandidate(x))
				.ToArray();
		}

		static bool IsExeOrDll(string path) {
			var ext = Path.GetExtension(path);
			return ext == ".dll" || ext == ".exe";
		}
	}
}
