namespace java.lang
{
    public static class JavaUtil
    {
        public static bool equals(this object one, object other)
        {
            return one.Equals(other);
        }

        public static int hashCode(this object one)
        {
            return one.GetHashCode();
        }

        public static string toString(this object one)
        {
            return one.ToString();
        }
    }
}