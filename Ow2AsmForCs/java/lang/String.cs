using System;

namespace java.lang
{
    public static class String_Util
    {
        public static int length(this string _string) => _string.Length;
        public static char[] toCharArray(this string _string) => _string.ToCharArray();
        public static char charAt(this string _string, int index) => _string[index];
    }
}