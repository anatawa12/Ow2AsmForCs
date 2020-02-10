using System.Runtime.CompilerServices;
using org.objectweb.asm;

namespace java.lang
{
    public static class SystemJ
    {
        public static void arraycopy<T>(T[] src, int srcPos, T[] dst, int dstPos, int length)
        {
            
        }

        public static int identityHashCode(object label)
        { 
            return RuntimeHelpers.GetHashCode(label);
        }
    }
}
