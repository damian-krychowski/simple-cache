using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.ExtensionMethods
{
    internal static class ExpressionExtensionMethods
    {
        public static bool Comapre(this Expression expression, Expression toCompare)
        {
            var original = expression.ToString().RemoveTillFirst('.');
            var compared = toCompare.ToString().RemoveTillFirst('.');
            return original == compared;
        }
    }
}
