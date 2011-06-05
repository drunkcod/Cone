using System.Collections.Generic;

namespace Cone.Core
{
    public interface IConeRowTestBuilder<T>
    {
        string NameFor(object[] parameters);
        T NewRow(string name, object[] parameters, TestStatus status);
    }

    public static class ConeRowTestBuilder
    {
        public static IEnumerable<T> BuildFrom<T>(this IConeRowTestBuilder<T> self, IEnumerable<IRowData> rows) {
            foreach (var row in rows) { 
                var parameters = row.Parameters;
                var rowName = row.DisplayAs ?? self.NameFor(parameters);
                var rowTest = self.NewRow(rowName, parameters, row.IsPending ? TestStatus.Pending : TestStatus.ReadyToRun);
                yield return rowTest;
            }
        }
    }
}