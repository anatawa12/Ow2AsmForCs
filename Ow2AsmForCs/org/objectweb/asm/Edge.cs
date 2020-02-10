using System;
using java.lang;

namespace org.objectweb.asm {
internal class Edge {
static readonly internal int NORMAL = 0;
static readonly internal int EXCEPTION = 0x7FFFFFFF;
internal int info;
internal Label successor;
internal Edge next;
}
}
