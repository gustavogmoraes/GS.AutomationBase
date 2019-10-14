using System;
using System.Collections.Generic;
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
    }
}
