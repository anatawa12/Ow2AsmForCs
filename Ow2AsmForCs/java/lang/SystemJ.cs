using System;
using System.Runtime.CompilerServices;
using org.objectweb.asm;

namespace java.lang
{
    public static class SystemJ
    {
        public static void arraycopy<T>(T[] src, int srcPos, T[] dst, int dstPos, int length)
        {
            if (src == null) 
                throw new ArgumentNullException(nameof(src));
            if (dst == null) 
                throw new ArgumentNullException(nameof(src));
            if (srcPos < 0) 
                throw new IndexOutOfRangeException($"{nameof(srcPos)} < 0");
            if (dstPos < 0) 
                throw new IndexOutOfRangeException($"{nameof(dstPos)} < 0");
            if (src.Length < srcPos+length) 
                throw new IndexOutOfRangeException($"{nameof(src)}.{src.Length} < {nameof(srcPos)}+{nameof(length)}");
            if (dst.Length < dstPos+length)
                throw new IndexOutOfRangeException($"{nameof(dst)}.{dst.Length} < {nameof(dstPos)}+{nameof(length)}");

            if (!ReferenceEquals(dst, src) || srcPos < dstPos)
            {
                for (int i = 0; i < length; i++)
                    dst[srcPos + i] = src[dstPos + i];
            }
            else
            {
                for (int i = length - 1; 0 <= i; i--)
                    dst[srcPos + i] = src[dstPos + i];
            }
        }

        public static int identityHashCode(object label)
        { 
            return RuntimeHelpers.GetHashCode(label);
        }
    }
}
