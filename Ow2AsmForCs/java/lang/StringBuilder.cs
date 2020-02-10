namespace java.lang
{
    public class StringBuilder
    {
        private readonly global::System.Text.StringBuilder _builder;

        
        public StringBuilder()
        {
            _builder = new global::System.Text.StringBuilder();
        }

        public StringBuilder(string input) : this()
        {
            append(input);
        }

        public StringBuilder(int initialSize)
        {
            _builder = new System.Text.StringBuilder(initialSize);
        }

        public StringBuilder append(object p0)
        {
            _builder.Append(p0);
            return this;
        }

        public StringBuilder append(char[] buf, int off, int len)
        {
            _builder.Append(buf, off, len);
            return this;
        }
    }
}