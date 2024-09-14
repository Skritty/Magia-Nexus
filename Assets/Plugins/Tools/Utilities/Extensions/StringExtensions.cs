using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    public static class StringExtensions
    {
        const string NULL_FILLER = "<Null>";
        const string EMPTY_FILLER = "<NONE>";

        /// <summary>
        /// Replaces the string if null or empty. Defaults to "&lt;Null&gt;" and "&lt;NONE&gt;".
        /// </summary>
        /// <param name="text"></param>
        /// <param name="emptyText"></param>
        /// <param name="nullText"></param>
        /// <returns></returns>
        public static string BackFill(this string text, string emptyText=EMPTY_FILLER, string nullText= NULL_FILLER)
        {
            if(text == null)
                return nullText;
            else if(text.NullIfEmpty() == null)
                return emptyText;
            else
                return text;
        }

        public static string SplitCamelCase(this string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }
}
