using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities {

    /// <summary>
    /// Provides a method for generating a side-by-side comparison of file strings.
    /// Can be used with formatted JSON, formatted XML, or any other file string that
    /// is broken down into separate lines in a standard format with standard white space.
    /// </summary>
    public class FileStringComparer {

        /// <summary>
        /// Returns two file strings as side-by-side strings, and places an "X"
        /// next to each row where the two file strings are not equal
        /// </summary>
        /// <param name="fileString1">The first file string to compare</param>
        /// <param name="fileString2">The second file string to compare</param>
        /// <returns>A single string of side-by-side file strings</returns>
        public static string GetSideBySideFileStrings(string fileString1, string fileString2,
            string header1, string header2) {

            //instantiate a new StringBuilder, which allows appending strings efficiently
            var sb = new StringBuilder();

            //split the strings into individual lines and combine into a two-dimensional array
            var strLists = new string[][] {
                GetLines(fileString1), GetLines(fileString2)
            };

            //get the maximum lengths of each line
            var maxLens = new int[] { GetMaxLineLength(strLists[0]), GetMaxLineLength(strLists[1]) };

            //print hard line
            sb.AppendLine("".PadRight(maxLens[0], '-') + "---" + "".PadRight(maxLens[1], '-'));

            //print header line
            sb.AppendLine(PadCenter(header1, maxLens[0]) + "   " + PadCenter(header2, maxLens[1]));

            //print hard line
            sb.AppendLine("".PadRight(maxLens[0], '-') + "---" + "".PadRight(maxLens[1], '-'));

            //put corresponding lines from each string on the same line
            //do this for all lines where both strings have the same number of lines
            for (int i = 0; i < strLists[0].Length && i < strLists[1].Length; i++) {
                sb.AppendLine(strLists[0][i] + " | " + strLists[1][i] + " | " + ((strLists[0][i].Trim() != strLists[1][i].Trim()) ? "X" : ""));
            }
            //add extra lines from the second string (if any)
            for (int i = strLists[0].Length; i < strLists[1].Length; i++) {
                sb.AppendLine("".PadRight(maxLens[0]) + " | " + strLists[1][i] + " | " + "X" );
            }
            //add extra lines from the first string (if any)
            for (int i = strLists[1].Length; i < strLists[0].Length; i++) {
                sb.AppendLine(strLists[0][i] + " | " + "".PadRight(maxLens[1]) + " | " + "X");
            }

            //call ToString() on the string builder object to return a single string
            return sb.ToString();
        }

        /// <summary>
        /// Centers text by left-padding and right-padding with spaces
        /// From https://stackoverflow.com/a/17590723
        /// </summary>
        /// <param name="source">The source string</param>
        /// <param name="length">overall Length desired</param>
        /// <returns></returns>
        private static string PadCenter(string source, int length) {
            int spaces = length - source.Length;
            int padLeft = spaces / 2 + source.Length;
            return source.PadLeft(padLeft).PadRight(length);

        }

        /// <summary>
        /// Splits a string into individual lines and pads each line
        /// with spaces such that all lines are the same length
        /// </summary>
        /// <param name="fileString">The input file string</param>
        /// <returns>an array of strings of the same length, with each
        /// string representing an individual line</returns>
        private static string[] GetLines(string fileString) {
            string str2 = fileString.Replace("\r\n", "\n");
            var strList = str2.Split("\n");
            var maxLen = GetMaxLineLength(strList);

            for(int i = 0; i < strList.Length; i++) {
                strList[i] = strList[i].PadRight(maxLen);
            }
            return strList;
        }

        /// <summary>
        /// Returns the maximum length of an array of strings
        /// </summary>
        /// <param name="strList">the input length</param>
        /// <returns>Maximum length of array</returns>
        private static int GetMaxLineLength(string[] strList) {
            var maxLen = 0;
            foreach (string s in strList)
                if (s.Length > maxLen)
                    maxLen = s.Length;
            return maxLen;
        }

    }
}
