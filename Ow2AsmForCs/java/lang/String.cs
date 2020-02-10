using System;

namespace java.lang
{
    public static class String_Util
    {
        public static int length(this string _string) => _string.Length;
        public static char[] toCharArray(this string _string) => _string.ToCharArray();
        public static char charAt(this string _string, int index) => _string[index];
        public static int indexOf(this string _string, char search, int startIndex = 0) => _string.IndexOf(search);
        public static string substring(this string _string, int begin) => _string.Substring(begin);
        public static string substring(this string _string, int begin, int end) => _string.Substring(begin, begin + end);
        public static string replace(this string _string, char from, char to) => _string.Replace(from, to);
    }
}