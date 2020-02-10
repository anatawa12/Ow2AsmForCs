using System;
using java.lang;

namespace org.objectweb.asm {
internal class Edge {
internal const int NORMAL = 0;
internal const int EXCEPTION = 0x7FFFFFFF;
internal int info;
internal Label successor;
internal Edge next;
}
}
