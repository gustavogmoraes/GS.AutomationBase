using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationBase
{
    public static class StringExtensions
    {
        public static string Between(this string input, string firstString, string lastString)
        {
            return input.Split(new[] { firstString }, StringSplitOptions.None)[1]
                        .Split(new[] { lastString }, StringSplitOptions.None)[0]
                        .Trim();
        }

        public static List<string> SplitIntoChunks(this string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize))
                .ToList();
        }
    }
}
