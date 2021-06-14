using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationBase.General.Extensions
{
    public static class StringExtensions
    {
        public static string Between(this string input, string firstString, string lastString)
        {
            var p1 = input.IndexOf(firstString, StringComparison.Ordinal) + firstString.Length;
            var p2 = input.IndexOf(lastString, p1, StringComparison.Ordinal);

            return lastString == "" 
                ? input.Substring(p1) 
                : input.Substring(p1, p2 - p1);
        }

        public static List<string> SplitIntoChunks(this string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize))
                .ToList();
        }
    }
}
