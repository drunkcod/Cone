﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public class Verify
    {
        public static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        static readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        readonly Expression body;
        bool outcome;

        static Verify From(Expression body) {
            switch (body.NodeType) {
                case ExpressionType.Not:
                    var x = From(((UnaryExpression)body).Operand);
                    x.outcome = !x.outcome;
                    return x;
                default: 
                    if(UnsupportedExpressionType(body.NodeType))
                        throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
                    return new Verify(body);
            }
        }

        static bool UnsupportedExpressionType(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Call: return false;
                case ExpressionType.Constant: return false;
                case ExpressionType.Equal: return false;
                case ExpressionType.NotEqual: return false;
                case ExpressionType.MemberAccess: return false;
            }
            return true;
        }

        Verify(Expression body){
            this.body = body;
            this.outcome = body.NodeType != ExpressionType.NotEqual;
        }

        void Check() {
            var expect = Lambda(body).Compile()();
            if(expect.Check() != outcome)
                ExpectationFailed(expect.Format(Formatter));
        }

        public static void That(Expression<Func<bool>> expr) {
            From(expr.Body).Check();
        }

        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            try {
                expr.Compile()();
                ExpectationFailed(Formatter.Format(expr) + " didn't raise an exception.");
                return null;
            } catch (TException expected) {
                return expected;
            } catch (Exception e) {
                var message = string.Format("{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}",
                    Formatter.Format(expr), typeof(TException), e.GetType());
                ExpectationFailed(message);
                return null;
            }
        }

        public static Expression<Func<Expect>> Lambda(Expression body) {
            var binary = body as BinaryExpression;
            if (binary != null)
                return BinaryExpect.Lambda(binary);
            return Expect.Lambda(body);
        }

    }
}
