﻿using System;
using System.Linq.Expressions;
using Cone.Expectations;

namespace Cone
{
    [Describe(typeof(StringMethodExpect))]
    public class StringContainsSpec
    {
        public void check_success() {
            Expression<Func<bool>> body = () => "Hello".Contains("ll");
            IExpect expect = ContainsExpect(body, "Hello", "ll");
            Verify.That(() => expect.Check().Equals(new ExpectResult { Success = true, Actual = "Hello" }));
        }

        public void check_fail() {
            Expression<Func<bool>> body = () => "123".Contains("ABC");
            IExpect expect = ContainsExpect(body, "123", "ABC");
            Verify.That(() => expect.Check().Equals(new ExpectResult { Success = false, Actual = "123" }));
        }

        public void example() {
            Verify.That(() => "Hello World".Contains("World"));
        }

        public void property_example() {
            var foo = this;
            Verify.That(() => foo.HelloWorld.Contains("World"));
        }

        StringMethodExpect ContainsExpect(Expression body, string actual, string value) {
            return new StringMethodExpect(_ => "string containing", body, typeof(string).GetMethod("Contains"), actual, new[]{ value });
        }

        string HelloWorld { get { return "Hello World"; } }
    }
}