namespace Cone
{
    [Describe(typeof(StringExtensions))]
    public class StringExtensionsSpec
    {
        [Row("a", "b", 0)
        ,Row("aaa", "aab", 2)
        ,Row("aaa", "aaa", -1)
        ,Row("aa", "a", 1)
        ,Row("a", "aa", 1)]
        public void IndexOfFirstDifference(string self, string other, int expected) {
            Verify.That(() => self.IndexOfDifference(other) == expected);
        }
    }
}
