using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cone.Samples
{
    public class Examples : IEnumerable<IRowTestData>
    {
        readonly ConeTestNamer testNamer = new ConeTestNamer();
        readonly MethodInfo method;

        readonly List<IRowTestData> rows = new List<IRowTestData>();

        protected Examples(MethodInfo method) {
            this.method = method;
        }

        public IEnumerator<IRowTestData> GetEnumerator() { return rows.GetEnumerator(); }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return rows.GetEnumerator(); }
    
        protected void AddRow(params object[] parameters) {
            rows.Add(new RowTestData(method, parameters).SetName(testNamer.NameFor(method, parameters)));
        }
    }

    public class Examples<TArg0, TArg1, TArg2> : Examples
    {
        public Examples(Action<TArg0, TArg1, TArg2> action): base(action.Method) { }
        public void Add(TArg0 arg0, TArg1 arg1, TArg2 arg2) { AddRow(arg0, arg1, arg2); }
    }

    [Feature("Free Delivery")]
    public class FreeDeliveryFeature
    {
        [Context("is offered to VIP customers with more than 5 books in the cart")]
        public class OfferedToVIPCustomers
        {
            [DisplayAs("{0,-13} {1,-8} {2}")]
            public void Example(string customerType, int bookCount, [DisplayClass(typeof(BoolDisplay), "Yes!", "No.")]bool freeDelivery) { }

            public Examples Rows() { return new Examples<string, int, bool>(Example) {
                    { "Regular" , 10, false },
                    {  "VIP"    ,  5, false },
                    {  "VIP"    ,  6, true  }
                };
            }
        }
    }
}
