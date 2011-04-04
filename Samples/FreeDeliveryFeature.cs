namespace Cone.Samples
{
    [Feature("Free Delivery")]
    public class FreeDeliveryFeature
    {
        [Context("is offered to VIP customers with more than 5 books in the cart")]
        public class OfferedToVIPCustomers
        {
            [DisplayAs("{0,-13} {1,-8} {2}")]
            public void Example(string customerType, int bookCount, [DisplayClass(typeof(BoolDisplay), "Yes!", "No.")]bool freeDelivery) { }

            public Examples Rows() { 
                return new Examples<string, int, bool>(Example) {
                    { "Regular" , 10, false },
                    {  "VIP"    ,  5, false },
                    {  "VIP"    ,  6, true  }
                };
            }
        }
    }
}
