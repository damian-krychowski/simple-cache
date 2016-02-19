using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.ExtensionMethods
{
    internal static class StringExtensionMethods
    {
        public static string RemoveTillFirst(this string toChange, char tillFirst)
        {
            var index = toChange.IndexOf(tillFirst);
            return toChange.Substring(index);
        }

    }
}
